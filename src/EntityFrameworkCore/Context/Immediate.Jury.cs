using Markdig;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SatelliteSite.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Xylab.Contesting.Entities;
using Xylab.Contesting.Models;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Models;
using Xylab.Polygon.Storages;

namespace Xylab.Contesting.Services
{
    public partial class ImmediateContestContext : IJuryContext
    {
        public virtual Task<IReadOnlyList<(IPAddress Address, int Subnet)>?> ListIpRangesAsync()
        {
            return Task.FromResult<IReadOnlyList<(IPAddress Address, int Subnet)>?>(
                Contest.Settings.IpRanges?.Select(Convert).ToList());

            static (IPAddress Address, int Subnet) Convert(string range)
            {
                var idx = range.IndexOf('/');
                return (
                    IPAddress.Parse(range[..idx]),
                    int.Parse(range[(idx + 1)..]));
            }
        }

        public virtual async Task<IReadOnlyList<Language>> ListLanguagesAsync(bool filtered = true)
        {
            var langs = await Polygon.Languages.ListAsync(filtered ? true : default(bool?));
            if (filtered && Contest.Settings.Languages != null)
            {
                langs = langs.Where(l => Contest.Settings.Languages.Contains(l.Id)).ToList();
            }

            return langs;
        }

        public virtual async Task<Language?> FindLanguageAsync(string? langid, bool filtered = true)
        {
            if (langid == null) return null;
            var lang = await Polygon.Languages.FindAsync(langid);
            if (lang == null) return null;

            if (filtered && Contest.Settings.Languages != null)
            {
                return lang.AllowSubmit && Contest.Settings.Languages.Contains(lang.Id) ? lang : null;
            }
            else
            {
                return lang;
            }
        }

        public virtual async Task<ContestWrapper> UpdateContestAsync(Expression<Func<Contest, Contest>> updateExpression)
        {
            await Ccs.UpdateAsync(Contest.Id, updateExpression);
            var @new = (await Ccs.FindAsync(Contest.Id))!;
            await Mediator.Publish(new Events.ContestUpdateEvent(Contest, @new, this));
            return @new;
        }

        public virtual async Task<ServerStatus> GetJudgeQueueAsync()
        {
            var lists = await Polygon.Judgings.GetJudgeQueueAsync(Contest.Id);
            return lists.SingleOrDefault() ?? new ServerStatus { ContestId = Contest.Id };
        }

        public virtual Task<Dictionary<int, (string, JuryLevel)>> ListJuriesAsync()
        {
            int cid = Contest.Id;
            return Db.ContestJuries
                .Where(j => j.ContestId == cid)
                .Join(Db.Users, j => j.UserId, u => u.Id, (j, u) => new { u.Id, u.UserName, j.Level })
                .ToDictionaryAsync(k => k.Id, v => (v.UserName, v.Level));
        }

        public virtual Task AssignJuryAsync(IUser user, JuryLevel level)
        {
            int cid = Contest.Id, userid = user.Id;
            return Db.ContestJuries.UpsertAsync(
                () => new Jury { ContestId = cid, UserId = userid, Level = level });
        }

        public virtual Task UnassignJuryAsync(IUser user)
        {
            int cid = Contest.Id, userid = user.Id;
            return Db.ContestJuries
                .Where(j => j.ContestId == cid && j.UserId == userid)
                .BatchDeleteAsync();
        }

        public virtual Task<List<Event>> ListEventsAsync(string[]? type, int after)
        {
            int cid = Contest.Id;
            return Db.ContestEvents
                .Where(e => e.ContestId == cid && e.Id > after)
                .WhereIf(type != null, e => type!.Contains(e.EndpointType))
                .ToListAsync();
        }

        public virtual Task CleanEventsAsync()
        {
            int cid = Contest.Id;
            return Db.ContestEvents
                .Where(e => e.ContestId == cid)
                .BatchDeleteAsync();
        }

        public virtual Task<Event?> GetMaxEventAsync()
        {
            int cid = Contest.Id;
            return Db.ContestEvents
                .Where(e => e.ContestId == cid)
                .OrderByDescending(e => e.Id)
                .FirstOrDefaultAsync();
        }

        public virtual async Task<string> GetReadmeAsync(bool source)
        {
            var io = Get<IContestFileProvider>();
            if (source)
            {
                var fileInfo = await io.GetReadmeSourceAsync(Contest.Id);
                return await fileInfo.ReadAsStringAsync() ?? string.Empty;
            }
            else
            {
                var fileInfo = await io.GetReadmeAsync(Contest.Id);
                return await fileInfo.ReadAsStringAndCacheAsync() ?? string.Empty;
            }
        }

        public virtual async Task SetReadmeAsync(string source)
        {
            source ??= "";
            var io = Get<IContestFileProvider>();
            var md = Get<IMarkdownService>();
            var document = md.Parse(source);

            await io.WriteReadmeSourceAsync(Contest.Id, source);
            await io.WriteReadmeAsync(Contest.Id, md.RenderAsHtml(document));
        }

        public virtual async Task<IReadOnlyDictionary<string, object>> GetUpdatesAsync()
        {
            int cid = Contest.Id;
            var clarifications = await Db.Clarifications.CountAsync(c => c.ContestId == cid && !c.Answered);
            var rejudgings = await Polygon.Rejudgings.CountUndoneAsync(Contest.Id);
            var teams = await Db.Teams.CountAsync(t => t.ContestId == cid && t.Status == 0);

            return new Dictionary<string, object>
            {
                [nameof(clarifications)] = clarifications,
                [nameof(teams)] = teams,
                [nameof(rejudgings)] = rejudgings
            };
        }

        public Task<IPagedList<Auditlog>> ViewLogsAsync(int page, int pageCount)
        {
            return Get<SatelliteSite.Services.IAuditlogger>()
                .ViewLogsAsync(Contest.Id, page, pageCount);
        }

        public Task UpdateSettingsAsync(ContestSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var settingsResult = settings.ToJson();
            return UpdateContestAsync(_ => new Contest { SettingsJson = settingsResult });
        }
    }
}
