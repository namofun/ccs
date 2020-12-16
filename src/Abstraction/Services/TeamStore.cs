using Ccs.Entities;
using Ccs.Models;
using SatelliteSite.IdentityModule.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Tenant.Entities;

namespace Ccs.Services
{
    /// <summary>
    /// The storage interface for <see cref="Team"/> and <see cref="Member"/>.
    /// </summary>
    /// <remarks>Note that all store interfaces shouldn't cache the result.</remarks>
    public interface ITeamStore
    {
        /// <summary>
        /// Count the pending teams.
        /// </summary>
        /// <param name="contest">The contest.</param>
        /// <returns>The task for counting.</returns>
        Task<int> CountPendingAsync(Contest contest);

        /// <summary>
        /// List the members with team ID as key and username as values.
        /// </summary>
        /// <param name="contest">The contest.</param>
        /// <returns>The task for lookups.</returns>
        Task<ILookup<int, string>> ListMembersAsync(Contest contest);

        /// <summary>
        /// Create a team with specified users.
        /// </summary>
        /// <param name="team">The team.</param>
        /// <param name="users">The users to join.</param>
        /// <returns>The task for creating team.</returns>
        Task<Team> CreateAsync(Team team, IEnumerable<IUser>? users = null);

        /// <summary>
        /// Find team by team ID.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <returns>The task for fetching team entity.</returns>
        Task<Team?> FindByIdAsync(int cid, int teamid);

        /// <summary>
        /// Find team by user ID.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="uid">The user ID.</param>
        /// <returns>The task for fetching team entity.</returns>
        Task<Team?> FindByUserAsync(int cid, int uid);

        /// <summary>
        /// Update the team.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <param name="expression">The update expression.</param>
        /// <returns>The task for updating teams.</returns>
        Task UpdateAsync(int cid, int teamid, Expression<Func<Team, Team>> expression);

        /// <summary>
        /// Load the scoreboard.
        /// </summary>
        /// <param name="contest">The contest.</param>
        /// <returns>The scoreboard data.</returns>
        Task<ScoreboardModel> LoadScoreboardAsync(Contest contest);

        /*

        [Obsolete]
        Task<List<T>> ListAsync<T>(int cid,
            Expression<Func<Team, T>> selector,
            Expression<Func<Team, bool>>? predicate = null,
            (string, TimeSpan)? cacheTag = null);

        [Obsolete]
        Task<T> FindAsync<T>(int cid, int tid,
            Expression<Func<Team, T>> selector);

        [Obsolete]
        Task<HashSet<int>> ListRegisteredAsync(int uid);

        [Obsolete]
        Task<List<Member>> ListRegisteredWithDetailAsync(int uid);

        [Obsolete]
        Task<List<Affiliation>> ListAffiliationAsync(int cid, bool filtered = true);

        [Obsolete]
        Task<HashSet<int>> ListMemberUidsAsync(int cid);

        [Obsolete]
        Task<List<Category>> ListCategoryAsync(int cid, bool? requirePublic = null);

        [Obsolete]
        Task<Dictionary<int, string>> ListNamesAsync(int cid);

        [Obsolete]
        Task<Dictionary<int, (int ac, int tot)>> StatisticsSubmissionAsync(int cid, int teamid);

        [Obsolete]
        Task<IEnumerable<int>> DeleteAsync(Team team);
        */
    }
}
