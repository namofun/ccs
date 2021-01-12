using Ccs.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
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
            if (!string.IsNullOrEmpty(Contest.Languages))
            {
                var available = Contest.Languages!.AsJson<string[]>() ?? Array.Empty<string>();
                langs = langs.Where(l => available.Contains(l.Id)).ToList();
            }

            return langs;
        }

        public virtual async Task<Contest> UpdateContestAsync(Expression<Func<Contest, Contest>> updateExpression)
        {
            await Ccs.UpdateAsync(Contest.Id, updateExpression);
            return await Ccs.FindAsync(Contest.Id);
        }

        public virtual async Task<HashSet<int>> FetchJuryAsync()
        {
            int cid = Contest.Id;
            var result = new HashSet<int>();
            var query = await Db.ContestJuries
                .Where(j => j.ContestId == cid)
                .Select(j => j.UserId)
                .ToListAsync();
            return new HashSet<int>(query);
        }

        public virtual Task AssignJuryAsync(IUser user)
        {
            int cid = Contest.Id, userid = user.Id;
            return Db.ContestJuries.UpsertAsync(
                new { cid, userid },
                s => new Jury { ContestId = cid, UserId = userid });
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

        public virtual Task<string> GetReadmeAsync(bool source)
        {
            throw new NotImplementedException();
        }

        public virtual Task SetReadmeAsync(string source)
        {
            throw new NotImplementedException();
        }
    }
}
