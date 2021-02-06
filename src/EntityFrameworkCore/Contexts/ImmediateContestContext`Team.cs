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
    public partial class ImmediateContestContext : IAnalysisContext, ITeamContext
    {
        private static readonly ConcurrentAsyncLock _teamLock = new ConcurrentAsyncLock();
        protected static readonly IReadOnlyDictionary<int, (int, int)> _emptyStat = new Dictionary<int, (int, int)>();

        private Task FixTeamCountAsync(int cid)
            => Db.Contests
                .Where(c => c.Id == cid)
                .BatchUpdateAsync(c => new Contest { TeamCount =
                    Db.Teams.Count(cp => cp.ContestId == c.Id && cp.Status == 1 && cp.Category.IsPublic) });

        public Task<List<Team>> ListTeamsAsync(Expression<Func<Team, bool>>? predicate = null)
        {
            int cid = Contest.Id;
            return Db.Teams
                .Where(t => t.ContestId == cid)
                .WhereIf(predicate != null, predicate!)
                .ToListAsync();
        }

        public virtual Task<Affiliation?> FetchAffiliationAsync(int id)
        {
            return Get<IAffiliationStore>().FindAsync(id)!;
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

        public virtual async Task<IReadOnlyDictionary<int, Affiliation>> FetchAffiliationsAsync(bool contestFiltered)
        {
            List<Affiliation> results;

            if (contestFiltered)
                results = await Get<IAffiliationStore>().ListAsync(
                    a => Db.Teams.Select(a => a.AffiliationId).Contains(a.Id));
            else
                results = await Get<IAffiliationStore>().ListAsync();

            return results.ToDictionary(a => a.Id);
        }

        public virtual async Task<IReadOnlyDictionary<int, Category>> FetchCategoriesAsync(bool contestFiltered)
        {
            List<Category> results;

            if (contestFiltered)
                results = await Get<ICategoryStore>().ListAsync(
                    a => Db.Teams.Select(a => a.CategoryId).Contains(a.Id));
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
            if (team.Status == 1) await FixTeamCountAsync(cid);
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
            if (origin.Status == 1 || status == 1) await FixTeamCountAsync(Contest.Id);
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

            if (team.Status == 1) await FixTeamCountAsync(cid);
            return list;
        }

        public virtual async Task<IReadOnlyDictionary<int, string>> FetchTeamNamesAsync()
        {
            int cid = Contest.Id;
            return await Db.Teams
                .Where(t => t.ContestId == cid && t.Status == 1)
                .Select(t => new { t.TeamId, t.TeamName })
                .ToDictionaryAsync(a => a.TeamId, a => a.TeamName);
        }

        public virtual async Task<ILookup<int, string>> FetchTeamMembersAsync()
        {
            var cid = Contest.Id;
            var results = await Db.TeamMembers
                .Where(t => t.ContestId == cid)
                .Join(Db.Users, m => m.UserId, u => u.Id, (m, u) => new { m.TeamId, u.UserName })
                .ToListAsync();
            return results.ToLookup(a => a.TeamId, a => a.UserName);
        }

        public virtual async Task<IEnumerable<string>> FetchTeamMemberAsync(Team team)
        {
            var (cid, teamid) = (team.ContestId, team.TeamId);
            return await Db.TeamMembers
                .Where(t => t.ContestId == cid && t.TeamId == teamid)
                .Join(Db.Users, m => m.UserId, u => u.Id, (m, u) => u.UserName)
                .ToListAsync();
        }

        public virtual async Task<IReadOnlyDictionary<int, Team>> FetchTeamsAsync()
        {
            var cid = Contest.Id;
            var affs = await FetchAffiliationsAsync(true);
            var cats = await FetchCategoriesAsync(true);

            return await Db.Teams
                .Where(t => t.ContestId == cid && t.Status == 1)
                .ToDictionaryAsync(
                    keySelector: t => t.TeamId,
                    elementSelector: t =>
                    {
                        t.Affiliation = affs[t.AffiliationId];
                        t.Category = cats[t.CategoryId];
                        return t;
                    });
        }

        public virtual async Task<ScoreboardModel> FetchScoreboardAsync()
        {
            int cid = Contest.Id;
            var value = await Db.Teams
                .Where(t => t.ContestId == cid && t.Status == 1)
                .Include(t => t.RankCache)
                .Include(t => t.ScoreCache)
                .ToDictionaryAsync(t => t.TeamId, t => (IScoreboardRow)t);
            return new ScoreboardModel(value);
        }

        public virtual async Task<IReadOnlyDictionary<int, (int, int)>> StatisticsAsync(Team? team)
        {
            if (team == null) return _emptyStat;
            var (cid, teamid) = (team.ContestId, team.TeamId);
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
            return Db.TeamMembers.UpsertAsync(
                new { cid = team.ContestId, teamid = team.TeamId, uid = user.Id },
                s => new Member { ContestId = s.cid, TeamId = s.teamid, UserId = s.uid, Temporary = temporary });
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
    }
}
