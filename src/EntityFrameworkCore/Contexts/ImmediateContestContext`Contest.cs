using Ccs.Entities;
using Markdig;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Polygon.Entities;
using Polygon.Storages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class ImmediateContestContext
    {
        public virtual async Task<IReadOnlyList<Language>> FetchLanguagesAsync()
        {
            var langs = await Polygon.Languages.ListAsync(true);
            if (Contest.Settings.Languages != null)
            {
                langs = langs.Where(l => Contest.Settings.Languages.Contains(l.Id)).ToList();
            }

            return langs;
        }

        public virtual async Task<Contest> UpdateContestAsync(Expression<Func<Contest, Contest>> updateExpression)
        {
            await Ccs.UpdateAsync(Contest.Id, updateExpression);
            return await Ccs.FindAsync(Contest.Id);
        }

        public virtual Task<Dictionary<int, string>> FetchJuryAsync()
        {
            int cid = Contest.Id;
            return Db.ContestJuries
                .Where(j => j.ContestId == cid)
                .Join(Db.Users, j => j.UserId, u => u.Id, (j, u) => new { u.Id, u.UserName })
                .ToDictionaryAsync(k => k.Id, v => v.UserName);
        }

        public virtual Task AssignJuryAsync(IUser user)
        {
            return Db.ContestJuries.UpsertAsync(
                new { cid = Contest.Id, userid = user.Id },
                s => new Jury { ContestId = s.cid, UserId = s.userid });
        }

        public virtual Task UnassignJuryAsync(IUser user)
        {
            int cid = Contest.Id, userid = user.Id;
            return Db.ContestJuries
                .Where(j => j.ContestId == cid && j.UserId == userid)
                .BatchDeleteAsync();
        }

        public virtual Task<List<Event>> FetchEventAsync(string[]? type, int after)
        {
            int cid = Contest.Id;
            return Db.ContestEvents
                .Where(e => e.ContestId == cid && e.Id > after)
                .WhereIf(type != null, e => type.Contains(e.EndpointType))
                .ToListAsync();
        }

        public virtual Task<int> MaxEventIdAsync()
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
    }
}
