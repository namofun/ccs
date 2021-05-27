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
    /// <summary>
    /// Provides contract for team controlling.
    /// </summary>
    public interface ITeamContext : IContestContext
    {
        /// <summary>
        /// Gets the specified affiliation.
        /// </summary>
        /// <param name="id">The affiliation ID.</param>
        /// <param name="filtered">Whether the found entity is filtered.</param>
        /// <returns>The task for fetching affiliation.</returns>
        Task<Affiliation?> FindAffiliationAsync(int id, bool filtered = true);

        /// <summary>
        /// Gets the specified affiliation.
        /// </summary>
        /// <param name="abbr">The affiliation ID.</param>
        /// <param name="filtered">Whether the found entity is filtered.</param>
        /// <returns>The task for fetching affiliation.</returns>
        Task<Affiliation?> FindAffiliationAsync(string abbr, bool filtered = true);

        /// <summary>
        /// Gets the specified category.
        /// </summary>
        /// <param name="id">The category ID.</param>
        /// <param name="filtered">Whether the found entity is filtered.</param>
        /// <returns>The task for fetching category.</returns>
        Task<Category?> FindCategoryAsync(int id, bool filtered = true);

        /// <summary>
        /// Gets the team members as a lookup dictionary.
        /// </summary>
        /// <returns>The task for getting this lookup.</returns>
        Task<ILookup<int, string>> GetTeamMembersAsync();

        /// <summary>
        /// Gets the team member for specified team.
        /// </summary>
        /// <param name="team">The team.</param>
        /// <returns>The task for getting member.</returns>
        Task<IEnumerable<string>> GetTeamMemberAsync(Team team);

        /// <summary>
        /// Gets the team member for specified team.
        /// </summary>
        /// <param name="team">The team.</param>
        /// <returns>The task for getting member.</returns>
        Task<List<Models.TeamMemberModel>> GetTeamMember2Async(Team team);

        /// <summary>
        /// Creates a team with users.
        /// </summary>
        /// <param name="team">The original team model.</param>
        /// <param name="users">The team members.</param>
        /// <returns>The task for creating contest teams, returning the team id.</returns>
        Task<Team> CreateTeamAsync(Team team, IEnumerable<IUser>? users);

        /// <summary>
        /// Updates team by expression.
        /// </summary>
        /// <param name="origin">The original team model.</param>
        /// <param name="expression">The expression for updating team.</param>
        /// <returns>The task for updating contest teams.</returns>
        Task UpdateTeamAsync(Team origin, Expression<Func<Team, Team>> expression);

        /// <summary>
        /// Updates team status.
        /// </summary>
        /// <param name="origin">The original team model.</param>
        /// <param name="status">The new team status.</param>
        /// <returns>The task for updating contest teams.</returns>
        Task UpdateTeamAsync(Team origin, int status);

        /// <summary>
        /// Deletes the team and returns existing members.
        /// </summary>
        /// <param name="origin">The original team.</param>
        /// <returns>The task for deleting contest teams.</returns>
        Task<IReadOnlyList<Member>> DeleteTeamAsync(Team origin);

        /// <summary>
        /// Attaches a user to the team if not attached.
        /// </summary>
        /// <param name="team">The contest team.</param>
        /// <param name="user">The identity user.</param>
        /// <param name="temporary">Whether this member is temporary account.</param>
        /// <returns>The task for attaching member.</returns>
        Task AttachMemberAsync(Team team, IUser user, bool temporary);

        /// <summary>
        /// Updates the specified member with expression.
        /// </summary>
        /// <param name="member">The member entity.</param>
        /// <param name="expression">The expression for updating member.</param>
        /// <returns>The task for updating member.</returns>
        Task UpdateMemberAsync(Member member, Expression<Func<Member, Member>> expression);

        /// <summary>
        /// Detaches the specified member from the team.
        /// </summary>
        /// <param name="member">The member entity.</param>
        /// <returns>The task for detaching member.</returns>
        Task DetachMemberAsync(Member member);

        /// <summary>
        /// Gets the scoreboard row.
        /// </summary>
        /// <returns>The task for getting scoreboard row.</returns>
        Task<List<ScoreboardRow>> GetScoreboardRowsAsync();

        /// <summary>
        /// Lists the teams with selected conditions.
        /// </summary>
        /// <param name="predicate">The conditions to match.</param>
        /// <returns>The task for listing entities.</returns>
        Task<List<Team>> ListTeamsAsync(Expression<Func<Team, bool>>? predicate = null);

        /// <summary>
        /// Statistics the submission status of team.
        /// </summary>
        /// <param name="team">The team to discover.</param>
        /// <returns>The task for statistical result of submissions.</returns>
        Task<IReadOnlyDictionary<int, (int, int)>> StatisticsAsync(Team? team);

        /// <summary>
        /// Statistics the submission status of whole contest.
        /// </summary>
        /// <returns>The task for statistical result of submissions (Accepted, Total, AcceptedTeam, TotalTeam).</returns>
        Task<IReadOnlyDictionary<int, (int, int, int, int)>> StatisticsGlobalAsync();

        /// <summary>
        /// Locks out the temporary accounts.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <returns>The task for locking out.</returns>
        Task<List<Member>> LockOutTemporaryAsync(IUserManager userManager);

        /// <summary>
        /// Gets the visible tenants.
        /// </summary>
        /// <returns>The task for getting tenants.</returns>
        Task<HashSet<int>> GetVisibleTenantsAsync();

        /// <summary>
        /// Allows the tenant.
        /// </summary>
        /// <param name="affiliation">The tenant entity.</param>
        /// <returns>The task for allowing tenant.</returns>
        Task AllowTenantAsync(Affiliation affiliation);

        /// <summary>
        /// Disallows the tenant.
        /// </summary>
        /// <param name="affiliation">The tenant entity.</param>
        /// <returns>The task for disallowing tenant.</returns>
        Task DisallowTenantAsync(Affiliation affiliation);

        /// <summary>
        /// Aggregate the teams and do analysis via database.
        /// </summary>
        /// <typeparam name="TKey">The aggregate key.</typeparam>
        /// <typeparam name="TValue">The aggregate value.</typeparam>
        /// <param name="grouping">The grouping selector.</param>
        /// <param name="aggregator">The result selector.</param>
        /// <returns>The task for aggregating results.</returns>
        Task<Dictionary<TKey, TValue>> AggregateTeamsAsync<TKey, TValue>(
            Expression<Func<Team, TKey>> grouping,
            Expression<Func<IGrouping<TKey, Team>, TValue>> aggregator)
            where TKey : notnull;

        /// <summary>
        /// Gets the monitor status for teams satisfying conditions. 
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The task for monitor status.</returns>
        Task<Monitor> GetMonitorAsync(Expression<Func<Team, bool>> predicate);
    }
}
