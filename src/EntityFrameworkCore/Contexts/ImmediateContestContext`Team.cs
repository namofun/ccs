using Ccs.Entities;
using Ccs.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Tenant.Entities;

namespace Ccs.Services
{
    public partial class ImmediateContestContext
    {
        public Task<List<T>> ListTeamsAsync<T>(Expression<Func<Team, T>> selector, Expression<Func<Team, bool>>? predicate = null) where T : class
        {
            int cid = Contest.Id;
            return TeamStore.ListAsync(selector, predicate.Combine(t => t.ContestId == cid));
        }

        public Task<Affiliation?> FetchAffiliationAsync(int id)
        {
            throw new NotImplementedException();
        }

        public virtual Task<ScoreboardModel> FetchScoreboardAsync()
        {
            return TeamStore.LoadScoreboardAsync(Contest.Id);
        }

        public virtual Task<Team?> FindTeamByIdAsync(int teamId)
        {
            return TeamStore.FindByIdAsync(Contest.Id, teamId);
        }

        public virtual Task<Member?> FindMemberByUserAsync(int userId)
        {
            return TeamStore.FindByUserAsync(Contest.Id, userId);
        }

        public virtual Task<IReadOnlyDictionary<int, Affiliation>> FetchAffiliationsAsync(bool contestFiltered)
        {
            throw new NotImplementedException();
        }

        public virtual Task<IReadOnlyDictionary<int, Category>> FetchCategoriesAsync(bool contestFiltered)
        {
            throw new NotImplementedException();
        }

        public virtual Task UpdateTeamAsync(Team origin, Expression<Func<Team>> expression)
        {
            return TeamStore.UpdateAsync(origin.ContestId, origin.TeamId, expression);
        }

        public virtual async Task<IReadOnlyDictionary<int, string>> FetchTeamNamesAsync()
        {
            var list = await TeamStore.ListAsync(t => new { t.TeamId, t.TeamName }, t => t.Status == 1);
            return list.ToDictionary(k => k.TeamId, k => k.TeamName);
        }

        public virtual async Task<IReadOnlyDictionary<int, (string Name, string Affiliation)>> FetchPublicTeamNamesWithAffiliationAsync()
        {
            var list = await TeamStore.ListAsync(t => new { t.TeamId, t.TeamName, t.Affiliation.Abbreviation }, t => t.Status == 1 && t.Category.IsPublic);
            return list.ToDictionary(k => k.TeamId, k => (k.TeamName, k.Abbreviation));
        }

        public virtual Task<IReadOnlyList<Member>> DeleteTeamAsync(Team origin)
        {
            return TeamStore.DeleteAsync(origin);
        }

        public virtual Task<ILookup<int, string>> FetchTeamMembersAsync()
        {
            return TeamStore.ListMembersAsync(Contest.Id);
        }

        public virtual Task<IEnumerable<string>> FetchTeamMemberAsync(Team team)
        {
            return TeamStore.ListMembersAsync(team);
        }

        public virtual Task<Team> CreateTeamAsync(Team team, IEnumerable<IUser>? users)
        {
            return TeamStore.CreateAsync(team, users);
        }
    }
}
