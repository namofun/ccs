using Ccs.Entities;
using Ccs.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Tenant.Entities;
using Tenant.Services;

namespace Ccs.Services
{
    public partial class ImmediateContestContext : ITeamContext
    {
        private static readonly ConcurrentAsyncLock _teamLock = new ConcurrentAsyncLock();
        protected static readonly IReadOnlyDictionary<int, (int, int)> _emptyStat = new Dictionary<int, (int, int)>();

        protected Task FixTeamCountAsync(int cid, bool up)
        {
            if (DateTimeOffset.Now.Ticks % 100 < 30)
            {
                return Db.Contests
                    .Where(c => c.Id == cid)
                    .BatchUpdateAsync(c => new Contest
                    {
                        TeamCount = Db.Teams
                            .Where(t => t.ContestId == c.Id && t.Status == 1)
                            .Join(Db.Categories, t => t.CategoryId, c => c.Id, (t, c) => new { t, c })
                            .Where(a => a.c.IsPublic)
                            .Count()
                    });
            }
            else
            {
                // Category.IsPublic is not considered yet
                // so the accurate update will hit in a larger probability
                int delta = up ? 1 : -1;
                return Db.Contests
                    .Where(c => c.Id == cid)
                    .BatchUpdateAsync(c => new Contest
                    {
                        TeamCount = c.TeamCount + delta
                    });
            }
        }

        public Task<List<Team>> ListTeamsAsync(Expression<Func<Team, bool>>? predicate = null)
        {
            int cid = Contest.Id;
            return Db.Teams
                .Where(t => t.ContestId == cid)
                .WhereIf(predicate != null, predicate!)
                .ToListAsync();
        }

        public virtual Task<Affiliation?> FindAffiliationAsync(int id, bool filtered = true)
        {
            return Db.Affiliations
                .Where(a => a.Id == id)
                .WhereIf(filtered, a => Db.Teams.Select(a => a.AffiliationId).Contains(a.Id))
                .FirstOrDefaultAsync()!;
        }

        public virtual Task<Affiliation?> FindAffiliationAsync(string id, bool filtered = true)
        {
            return Db.Affiliations
                .Where(a => a.Abbreviation == id)
                .WhereIf(filtered, a => Db.Teams.Select(a => a.AffiliationId).Contains(a.Id))
                .FirstOrDefaultAsync()!;
        }

        public virtual Task<Category?> FindCategoryAsync(int id, bool filtered = true)
        {
            return Db.Categories
                .Where(a => a.Id == id)
                .WhereIf(filtered, a => Db.Teams.Select(a => a.CategoryId).Contains(a.Id))
                .FirstOrDefaultAsync()!;
        }

        public virtual Task<Team?> FindTeamByIdAsync(int teamid)
        {
            int cid = Contest.Id;
            return Db.Teams
                .Where(t => t.ContestId == cid && t.TeamId == teamid)
                .SingleOrDefaultAsync()!;
        }

        public virtual Task<Member?> FindMemberByUserAsync(int userId)
        {
            int cid = Contest.Id;
            return Db.TeamMembers
                .Where(tu => tu.ContestId == cid && tu.UserId == userId)
                .SingleOrDefaultAsync()!;
        }

        public virtual async Task<IReadOnlyDictionary<int, Affiliation>> ListAffiliationsAsync(bool contestFiltered)
        {
            List<Affiliation> results;
            int cid = Contest.Id;

            if (contestFiltered)
                results = await Get<IAffiliationStore>().ListAsync(
                    a => Db.Teams
                        .Where(t => t.ContestId == cid)
                        .Select(a => a.AffiliationId)
                        .Contains(a.Id));
            else
                results = await Get<IAffiliationStore>().ListAsync();

            return results.ToDictionary(a => a.Id);
        }

        public virtual async Task<IReadOnlyDictionary<int, Category>> ListCategoriesAsync(bool contestFiltered)
        {
            List<Category> results;
            int cid = Contest.Id;

            if (contestFiltered)
                results = await Get<ICategoryStore>().ListAsync(
                    a => Db.Teams
                        .Where(t => t.ContestId == cid)
                        .Select(a => a.CategoryId)
                        .Contains(a.Id));
            else
                results = await Get<ICategoryStore>().ListAsync();

            return results.ToDictionary(a => a.Id);
        }

        public virtual async Task<Team> CreateTeamAsync(Team team, IEnumerable<IUser>? users)
        {
            int cid = team.ContestId;
            using var _lock = await _teamLock.LockAsync(cid);

            team.TeamId = 1 + await Db.Teams.CountAsync(tt => tt.ContestId == cid);
            Db.Teams.Add(team);

            if (users != null && users.Any())
            {
                foreach (var uid in users)
                {
                    Db.TeamMembers.Add(new Member
                    {
                        ContestId = team.ContestId,
                        TeamId = team.TeamId,
                        UserId = uid.Id,
                        Temporary = false
                    });
                }
            }

            await Db.SaveChangesAsync();
            if (team.Status == 1) await FixTeamCountAsync(cid, true);
            return team;
        }

        public virtual async Task UpdateTeamAsync(Team origin, Expression<Func<Team, Team>> expression)
        {
            var (cid, teamid) = (origin.ContestId, origin.TeamId);
            var affected = await Db.Teams
                .Where(t => t.ContestId == cid && t.TeamId == teamid)
                .BatchUpdateAsync(expression);

            if (affected != 1)
                throw new DbUpdateException();
        }

        public virtual async Task UpdateTeamAsync(Team origin, int status)
        {
            if (origin.Status == status) return;
            await UpdateTeamAsync(origin, t => new Team { Status = status });
            if (origin.Status == 1 || status == 1) await FixTeamCountAsync(Contest.Id, status == 1);
        }

        public virtual async Task<IReadOnlyList<Member>> DeleteTeamAsync(Team team)
        {
            var (cid, teamid) = (team.ContestId, team.TeamId);

            var affected = await Db.Teams
                .Where(t => t.ContestId == cid && t.TeamId == teamid)
                .BatchUpdateAsync(_ => new Team { Status = 3 });

            if (affected != 1)
                throw new DbUpdateException();

            var list = await Db.TeamMembers
                .Where(t => t.ContestId == cid && t.TeamId == teamid)
                .ToListAsync();

            await Db.TeamMembers
                .Where(t => t.ContestId == cid && t.TeamId == teamid)
                .BatchDeleteAsync();

            if (team.Status == 1) await FixTeamCountAsync(cid, false);
            return list;
        }

        public virtual async Task<IReadOnlyDictionary<int, string>> GetTeamNamesAsync()
        {
            int cid = Contest.Id;
            return await Db.Teams
                .Where(t => t.ContestId == cid && t.Status == 1)
                .Select(t => new { t.TeamId, t.TeamName })
                .ToDictionaryAsync(a => a.TeamId, a => a.TeamName);
        }

        public virtual Task<ILookup<int, string>> GetTeamMembersAsync()
        {
            int cid = Contest.Id;
            return Db.TeamMembers
                .Where(t => t.ContestId == cid)
                .Join(Db.Users, m => m.UserId, u => u.Id, (m, u) => new { m.TeamId, u.UserName })
                .ToLookupAsync(a => a.TeamId, a => a.UserName);
        }

        public virtual async Task<IEnumerable<string>> GetTeamMemberAsync(Team team)
        {
            int cid = team.ContestId, teamid = team.TeamId;
            return await Db.TeamMembers
                .Where(t => t.ContestId == cid && t.TeamId == teamid)
                .Join(Db.Users, m => m.UserId, u => u.Id, (m, u) => u.UserName)
                .ToListAsync();
        }

        public virtual async Task<IReadOnlyDictionary<int, AnalyticalTeam>> GetAnalyticalTeamsAsync()
        {
            int cid = Contest.Id;
            var affs = await ListAffiliationsAsync(true);
            var cats = await ListCategoriesAsync(true);

            return await Db.Teams
                .Where(t => t.ContestId == cid && t.Status == 1)
                .Select(t => new AnalyticalTeam(t.TeamId, t.TeamName, t.AffiliationId, t.CategoryId))
                .ToDictionaryAsync(t => t.TeamId, t => t.With(cats, affs));
        }

        public virtual async Task<ScoreboardModel> GetScoreboardAsync()
        {
            if (Contest.Kind == CcsDefaults.KindProblemset)
                return ScoreboardModel.Singleton;

            int cid = Contest.Id;

            var rankCaches = await Db.RankCache
                .AsNoTracking()
                .Where(t => t.ContestId == cid)
                .ToDictionaryAsync(a => a.TeamId);

            var scoreCaches = await Db.ScoreCache
                .AsNoTracking()
                .Where(t => t.ContestId == cid)
                .ToLookupAsync(a => a.TeamId, a => a);

            var teams = await Db.Teams
                .Where(t => t.ContestId == cid && t.Status == 1)
                .Select(t => new ScoreboardRow(t.TeamId, t.TeamName, t.CategoryId, t.AffiliationId))
                .ToDictionaryAsync(
                    a => a.TeamId,
                    v => v.With(rankCaches.GetValueOrDefault(v.TeamId), scoreCaches[v.TeamId]));

            return new ScoreboardModel(teams);
        }

        public virtual async Task<IReadOnlyDictionary<int, (int, int)>> StatisticsAsync(Team? team)
        {
            if (team == null) return _emptyStat;
            int cid = team.ContestId, teamid = team.TeamId;
            return await Db.SubmissionStatistics
                .Where(t => t.ContestId == cid && t.TeamId == teamid)
                .ToDictionaryAsync(s => s.ProblemId, s => (s.AcceptedSubmission, s.TotalSubmission));
        }

        public virtual async Task<IReadOnlyDictionary<int, (int, int, int, int)>> StatisticsGlobalAsync()
        {
            int cid = Contest.Id;
            return await Db.SubmissionStatistics
                .Where(t => t.ContestId == cid)
                .GroupBy(k => k.ProblemId, v => new { v.AcceptedSubmission, v.TotalSubmission })
                .Select(
                    selector: g => new
                    {
                        ProblemId = g.Key,
                        Accepted = g.Sum(a => a.AcceptedSubmission),
                        Total = g.Sum(a => a.TotalSubmission),
                        AcceptedTeams = g.Sum(a => a.AcceptedSubmission > 0 ? 1 : 0),
                        TotalTeams = g.Sum(a => a.TotalSubmission > 0 ? 1 : 0),
                    })
                .ToDictionaryAsync(
                    keySelector: s => s.ProblemId,
                    elementSelector: s => (s.Accepted, s.Total, s.AcceptedTeams, s.TotalTeams));
        }

        public virtual Task AttachMemberAsync(Team team, IUser user, bool temporary)
        {
            int cid = team.ContestId, teamid = team.TeamId, uid = user.Id;
            return Db.TeamMembers.UpsertAsync(
                () => new Member { ContestId = cid, TeamId = teamid, UserId = uid, Temporary = temporary });
        }

        public virtual Task UpdateMemberAsync(Member member, Expression<Func<Member, Member>> expression)
        {
            int cid = member.ContestId, teamid = member.TeamId, uid = member.UserId;
            return Db.TeamMembers
                .Where(m => m.ContestId == cid && m.TeamId == teamid && m.UserId == uid)
                .BatchUpdateAsync(expression);
        }

        public virtual Task DetachMemberAsync(Member member)
        {
            int cid = member.ContestId, teamid = member.TeamId, uid = member.UserId;
            return Db.TeamMembers
                .Where(m => m.ContestId == cid && m.TeamId == teamid && m.UserId == uid)
                .BatchDeleteAsync();
        }

        public virtual async Task<List<Member>> LockOutTemporaryAsync(IUserManager userManager)
        {
            int cid = Contest.Id;

            var query = Db.TeamMembers.Where(m => m.ContestId == cid && m.Temporary);
            await userManager.LockoutUsersAsync(query.Select(m => m.UserId));

            var members = await query.ToListAsync();
            var affected = await query.BatchDeleteAsync();

            return members;
        }

        public virtual Task<HashSet<int>> GetVisibleTenantsAsync()
        {
            int cid = Contest.Id;
            return Db.ContestTenants
                .Where(c => c.ContestId == cid)
                .Select(c => c.AffiliationId)
                .ToHashSetAsync();
        }

        public virtual Task AllowTenantAsync(Affiliation affiliation)
        {
            int cid = Contest.Id, affid = affiliation.Id;
            return Db.ContestTenants.UpsertAsync(
                () => new Visibility { AffiliationId = affid, ContestId = cid });
        }

        public virtual Task DisallowTenantAsync(Affiliation affiliation)
        {
            int cid = Contest.Id, affid = affiliation.Id;
            return Db.ContestTenants
                .Where(c => c.ContestId == cid && c.AffiliationId == affid)
                .BatchDeleteAsync();
        }

        public virtual Task<bool> IsTenantVisibleAsync(IEnumerable<int> tenants)
        {
            int cid = Contest.Id;
            return Db.ContestTenants
                .Where(c => c.ContestId == cid && tenants.Contains(c.AffiliationId))
                .AnyAsync();
        }

        public virtual Task<Dictionary<TKey, TValue>> AggregateTeamsAsync<TKey, TValue>(
            Expression<Func<Team, TKey>> grouping,
            Expression<Func<IGrouping<TKey, Team>, TValue>> aggregator)
            where TKey : notnull
        {
            int cid = Contest.Id;
            return Db.Teams
                .Where(t => t.ContestId == cid && t.Status == 1)
                .GroupBy(grouping)
                .Select(Aggregator<TKey, TValue>.CreateExpression(aggregator))
                .ToDictionaryAsync(k => k.Key, v => v.Value);
        }

        public virtual Task<List<TeamMemberModel>> GetTeamMember2Async(Team team)
        {
            int cid = team.ContestId, teamid = team.TeamId;
            return Db.TeamMembers
                .Where(m => m.ContestId == cid && m.TeamId == teamid)
                .Join(
                    inner: Db.Users,
                    outerKeySelector: m => m.UserId,
                    innerKeySelector: u => u.Id,
                    resultSelector: (m, u) => new TeamMemberModel(m.TeamId, m.UserId, u.UserName, m.LastLoginIp))
                .ToListAsync();
        }

        public virtual async Task<Monitor> GetMonitorAsync(Expression<Func<Team, bool>> predicate)
        {
            int cid = Contest.Id;
            var baseQuery = Db.Teams.Where(t => t.ContestId == cid).Where(predicate);
            var teams = await baseQuery.ToListAsync();

            var members = await baseQuery
                .Join(
                    inner: Db.TeamMembers,
                    outerKeySelector: t => new { t.ContestId, t.TeamId },
                    innerKeySelector: m => new { m.ContestId, m.TeamId },
                    resultSelector: (t, m) => m)
                .Join(
                    inner: Db.Users,
                    outerKeySelector: m => m.UserId,
                    innerKeySelector: u => u.Id,
                    resultSelector: (m, u) => new TeamMemberModel(m.TeamId, m.UserId, u.UserName, m.LastLoginIp))
                .ToListAsync();

            return new Monitor(teams, members);
        }
    }
}
