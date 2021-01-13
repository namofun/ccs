﻿using Ccs.Entities;
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
    public partial class ImmediateContestContext
    {
        private static readonly ConcurrentAsyncLock _teamLock = new ConcurrentAsyncLock();

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

        [Checked]
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
            return team;
        }

        [Checked]
        public virtual async Task UpdateTeamAsync(Team origin, Expression<Func<Team, Team>> expression)
        {
            var (cid, teamid) = (origin.ContestId, origin.TeamId);
            var affected = await Db.Teams
                .Where(t => t.ContestId == cid && t.TeamId == teamid)
                .BatchUpdateAsync(expression);

            if (affected != 1)
                throw new DbUpdateException();
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

        [Checked]
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
    }
}