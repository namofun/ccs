using Ccs.Entities;
using Ccs.Models;
using Markdig;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Polygon.Entities;
using Polygon.Models;
using Polygon.Storages;
using SatelliteSite.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class ImmediateContestContext : IJuryContext
    {
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

        public virtual Task<Dictionary<int, string>> ListJuriesAsync()
        {
            int cid = Contest.Id;
            return Db.ContestJuries
                .Where(j => j.ContestId == cid)
                .Join(Db.Users, j => j.UserId, u => u.Id, (j, u) => new { u.Id, u.UserName })
                .ToDictionaryAsync(k => k.Id, v => v.UserName);
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
                .WhereIf(type != null, e => type.Contains(e.EndpointType))
                .ToListAsync();
        }

        public virtual Task CleanEventsAsync()
        {
            int cid = Contest.Id;
            return Db.ContestEvents
                .Where(e => e.ContestId == cid)
                .BatchDeleteAsync();
        }

        public virtual Task<int> GetMaxEventIdAsync()
        {
            int cid = Contest.Id;
            return Db.ContestEvents
                .Where(e => e.ContestId == cid)
                .OrderByDescending(e => e.Id)
                .Select(e => e.Id)
                .FirstOrDefaultAsync(); ;
        }

        public virtual async Task<string> GetReadmeAsync(bool source)
        {
            string fileName = source ? $"c{Contest.Id}/readme.md" : $"c{Contest.Id}/readme.html";
            var fileInfo = await Get<IProblemFileProvider>().GetFileInfoAsync(fileName);
            return await fileInfo.ReadAsync() ?? string.Empty;
        }

        public virtual async Task SetReadmeAsync(string source)
        {
            source ??= "";
            var io = Get<IProblemFileProvider>();
            var md = Get<IMarkdownService>();
            var document = md.Parse(source);

            await io.WriteStringAsync($"c{Contest.Id}/readme.md", source);
            await io.WriteStringAsync($"c{Contest.Id}/readme.html", md.RenderAsHtml(document));
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
