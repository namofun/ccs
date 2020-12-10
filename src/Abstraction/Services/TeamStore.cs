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
        Task<Team> FindByIdAsync(int cid, int teamid);

        /// <summary>
        /// Find team by user ID.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="uid">The user ID.</param>
        /// <returns>The task for fetching team entity.</returns>
        Task<Team> FindByUserAsync(int cid, int uid);

        /// <summary>
        /// Batch create teams and re-generate passwords.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="contest">The contest.</param>
        /// <param name="affiliation">The affiliation.</param>
        /// <param name="category">The category.</param>
        /// <param name="teamNames">The team names.</param>
        /// <returns>The list for generated teams.</returns>
        Task<List<(Team team, string password)>> BatchCreateAsync(
            IUserManager userManager,
            Contest contest,
            Affiliation affiliation,
            Category category,
            string[] teamNames);

        /// <summary>
        /// Clear the batch created teams.
        /// </summary>
        /// <param name="contest">The contest.</param>
        /// <returns>The task for affected rows.</returns>
        Task<int> BatchClearAsync(Contest contest);

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
    }
}
