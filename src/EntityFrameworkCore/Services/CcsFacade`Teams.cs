using Ccs.Entities;
using Ccs.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class CcsFacade<TUser, TContext> : ITeamStore
    {
        private static readonly ConcurrentAsyncLock _teamLock = new ConcurrentAsyncLock();

        /*
        public async Task<HashSet<int>> ListRegisteredAsync(int uid)
        {
            var members = Members
                .Where(t => t.UserId == uid)
                .Select(t => t.ContestId)
                .AsAsyncEnumerable();

            var results = new HashSet<int>();
            await foreach (var i in members)
                results.Add(i);
            return results;
        }

        public Task<List<Member>> ListRegisteredWithDetailAsync(int uid)
        {
            return Members.Where(m => m.UserId == uid)
                .Include(m => m.Team)
                    .ThenInclude(m => m.Contest)
                .Include(m => m.Team)
                    .ThenInclude(m => m.Affiliation)
                .Include(m => m.Team)
                    .ThenInclude(m => m.Category)
                .ToListAsync();
        }

        public Task<T> FindAsync<T>(int cid, int tid,
            Expression<Func<Team, T>> selector)
        {
            return Teams
                .Where(t => t.ContestId == cid && t.TeamId == tid)
                .Select(selector)
                .SingleOrDefaultAsync();
        }

        public Task<List<T>> ListAsync<T>(int cid,
            Expression<Func<Team, T>> selector,
            Expression<Func<Team, bool>>? predicate,
            (string, TimeSpan)? cacheTag)
            where T : class
        {
            var query = Teams.Where(t => t.ContestId == cid && t.Status != 3);
            if (predicate != null) query = query.Where(predicate);
            var query2 = query.Select(selector);
            if (!cacheTag.HasValue) return query2.ToListAsync();
            return query2.CachedToListAsync(cacheTag.Value.Item1, cacheTag.Value.Item2);
        }

        public Task<Dictionary<int, string>> ListNamesAsync(int cid)
        {
            return Teams
                .Where(t => t.ContestId == cid && t.Status == 1)
                .Select(t => new { t.TeamId, t.TeamName })
                .CachedToDictionaryAsync(
                    keySelector: t => t.TeamId,
                    valueSelector: t => t.TeamName,
                    tag: $"`c{cid}`teams`names_dict",
                    timeSpan: TimeSpan.FromMinutes(10));
        }

        public Task<Dictionary<int, (int ac, int tot)>> StatisticsSubmissionAsync(int cid, int teamid)
        {
            return Context.Set<SubmissionStatistics>()
                .Where(s => s.ContestId == cid && s.TeamId == teamid)
                .CachedToDictionaryAsync(
                    keySelector: s => s.ProblemId,
                    valueSelector: s => (s.AcceptedSubmission, s.TotalSubmission),
                    $"`c{cid}`teams`{teamid}`substat", TimeSpan.FromMinutes(1));
        }
        */

        Task<Team?> ITeamStore.FindByIdAsync(int cid, int teamid)
        {
            return Teams
                .Where(t => t.ContestId == cid && t.TeamId == teamid)
                .SingleOrDefaultAsync()!;
        }

        Task<Member?> ITeamStore.FindByUserAsync(int cid, int uid)
        {
            return Members
                .Where(tu => tu.ContestId == cid && tu.UserId == uid)
                .SingleOrDefaultAsync()!;
        }

        Task<ScoreboardModel> ITeamStore.LoadScoreboardAsync(int cid)
        {
            return Context.CachedGetAsync($"`c{cid}`scoreboard", TimeSpan.FromSeconds(3),
            async () =>
            {
                var value = await Teams
                    .Where(t => t.ContestId == cid && t.Status == 1)
                    .Include(t => t.RankCache)
                    .Include(t => t.ScoreCache)
                    .ToDictionaryAsync(a => a.TeamId);

                var result = new ScoreboardModel
                {
                    //Data = value,
                    //RefreshTime = DateTimeOffset.Now,
                    //Statistics = new Dictionary<int, int>()
                };

                /*
                foreach (var (_, item) in value)
                {
                    foreach (var ot in item.ScoreCache)
                    {
                        var val = result.Statistics.GetValueOrDefault(ot.ProblemId);
                        if (ot.IsCorrectRestricted)
                            result.Statistics[ot.ProblemId] = ++val;
                    }
                }
                */

                return result;
            });
        }

        /*
        public Task<List<Affiliation>> ListAffiliationAsync(int cid, bool filtered = true)
        {
            var query = Context.Set<Affiliation>().AsQueryable();

            if (filtered)
            {
                var avail = Teams
                    .Where(t => t.ContestId == cid && t.Status == 1)
                    .Select(t => t.AffiliationId);
                query = query.Where(a => avail.Contains(a.Id));
            }

            return query.CachedToListAsync(
                tag: $"`c{cid}`teams`aff{(filtered ? 0 : 1)}",
                timeSpan: TimeSpan.FromMinutes(5));
        }

        public Task<List<Category>> ListCategoryAsync(int cid, bool? requirePublic = null)
        {
            if (requirePublic.HasValue)
            {
                var items = Teams
                    .Where(t => t.ContestId == cid && t.Status == 1)
                    .Select(t => t.CategoryId);
                var query = Context.Set<Category>()
                    .Where(c => items.Contains(c.Id));
                if (requirePublic.Value)
                    query = query.Where(tc => tc.IsPublic);

                return query.CachedToListAsync(
                    tag: $"`c{cid}`teams`cat`{(requirePublic.Value ? 2 : 1)}",
                    timeSpan: TimeSpan.FromMinutes(5));
            }
            else
            {
                return Context.Set<Category>().CachedToListAsync(
                    tag: $"`c{cid}`teams`cat`0",
                    timeSpan: TimeSpan.FromMinutes(5));
            }
        }
        */

        async Task ITeamStore.UpdateAsync(int cid, int teamid, Expression<Func<Team>> expression)
        {
            var affected = await Teams
                .Where(t => t.ContestId == cid && t.TeamId == teamid)
                .BatchUpdateAsync(Expression.Lambda<Func<Team, Team>>(expression.Body, Expression.Parameter(typeof(Team), "_")));

            if (affected != 1)
                throw new DbUpdateException();
        }

        async Task<IReadOnlyList<Member>> ITeamStore.DeleteAsync(Team team)
        {
            var affected = await Teams
                .Where(t => t.ContestId == team.ContestId && t.TeamId == team.TeamId)
                .BatchUpdateAsync(_ => new Team { Status = 3 });

            if (affected != 1)
                throw new DbUpdateException();

            var list = await Members
                .Where(tu => tu.ContestId == team.ContestId && tu.TeamId == team.TeamId)
                .ToListAsync();

            await Members
                .Where(tu => tu.ContestId == team.ContestId && tu.TeamId == team.TeamId)
                .BatchDeleteAsync();

            return list;
        }

        public async Task<HashSet<int>> ListMemberUidsAsync(int cid)
        {
            var members = Members
                .Where(m => m.ContestId == cid)
                .Select(m => m.UserId)
                .AsAsyncEnumerable();

            var results = new HashSet<int>();
            await foreach (var i in members)
                results.Add(i);
            return results;
        }

        Task<int> ITeamStore.CountPendingAsync(int cid)
        {
            return Teams
                .Where(t => t.Status == 0 && t.ContestId == cid)
                .CountAsync();
        }

        async Task<ILookup<int, string>> ITeamStore.ListMembersAsync(int cid)
        {
            var query =
                from m in Members
                where m.ContestId == cid
                join u in Users on m.UserId equals u.Id
                select new { m.TeamId, u.UserName };

            var results = await query.ToListAsync();
            return results.ToLookup(a => a.TeamId, a => a.UserName);
        }

        async Task<Team> ITeamStore.CreateAsync(Team team, IEnumerable<IUser>? users)
        {
            int cid = team.ContestId;
            using var _lock = await _teamLock.LockAsync(cid);

            team.TeamId = 1 + await Teams.CountAsync(tt => tt.ContestId == cid);
            Teams.Add(team);

            if (users != null)
            {
                foreach (var uid in users)
                {
                    Members.Add(new Member
                    {
                        ContestId = team.ContestId,
                        TeamId = team.TeamId,
                        UserId = uid.Id,
                        Temporary = false
                    });
                }
            }

            await Context.SaveChangesAsync();
            return team;
        }

        Task<List<T>> ITeamStore.ListAsync<T>(Expression<Func<Team, T>> selector, Expression<Func<Team, bool>>? predicate) where T : class
        {
            return Teams
                .WhereIf(predicate != null, predicate!)
                .Select(selector)
                .ToListAsync();
        }

        Task<IEnumerable<string>> ITeamStore.ListMembersAsync(Team team)
        {
            throw new NotImplementedException();
        }
    }
}
