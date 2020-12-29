using Ccs.Entities;
using Ccs.Models;
using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Tenant.Entities;

namespace Ccs.Services
{
    public class TeamStore<TUser, TContext> : ITeamStore
        where TContext : DbContext
        where TUser : SatelliteSite.IdentityModule.Entities.User, new()
    {
        private static AsyncLock TeamLock { get; } = new AsyncLock();

        public TContext Context { get; }

        DbSet<Team> Teams => Context.Set<Team>();

        DbSet<Member> Members => Context.Set<Member>();

        public TeamStore(TContext context)
        {
            Context = context;
        }

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

        public Task<Team?> FindByIdAsync(int cid, int teamid)
        {
            return Teams
                .Where(t => t.ContestId == cid && t.TeamId == teamid)
                .SingleOrDefaultAsync()!;
        }

        public Task<Member?> FindByUserAsync(int cid, int uid)
        {
            return Members
                .Where(tu => tu.ContestId == cid && tu.UserId == uid)
                .SingleOrDefaultAsync()!;
        }

        public Task<ScoreboardModel> LoadScoreboardAsync(int cid)
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

        public async Task UpdateAsync(int cid, int teamid, Expression<Func<Team>> activator)
        {
            var affected = await Teams
                .Where(t => t.ContestId == cid && t.TeamId == teamid)
                .BatchUpdateAsync(Expression.Lambda<Func<Team, Team>>(activator.Body, Expression.Parameter(typeof(Team), "_")));

            if (affected != 1)
                throw new DbUpdateException();
        }

        public async Task<IReadOnlyList<Member>> DeleteAsync(Team team)
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

        public Task<int> CountPendingAsync(Contest contest)
        {
            return Teams
                .Where(t => t.Status == 0 && t.ContestId == contest.Id)
                .CountAsync();
        }

        public Task<ILookup<int, string>> ListMembersAsync(Contest contest)
        {
            throw new NotImplementedException();
        }

        public async Task<Team> CreateAsync(Team team, IEnumerable<SatelliteSite.IdentityModule.Services.IUser>? users = null)
        {
            using var _lock = await TeamLock.LockAsync();
            int cid = team.ContestId;

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

        public Task<ScoreboardModel> LoadScoreboardAsync(Contest contest)
        {
            return Context.CachedGetAsync(
                tag: $"`c{contest.Id}`scoreboard",
                timeSpan: TimeSpan.FromSeconds(3),
                async context =>
                {
                    int cid = contest.Id;

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

        public Task<List<T>> ListAsync<T>(
            Expression<Func<Team, T>> selector,
            Expression<Func<Team, bool>>? predicate = null)
            where T : class
        {
            return Teams
                .WhereIf(predicate != null, predicate!)
                .Select(selector)
                .ToListAsync();
        }

        public Task<IEnumerable<string>> ListMembersAsync(Team team)
        {
            throw new NotImplementedException();
        }
    }
}
