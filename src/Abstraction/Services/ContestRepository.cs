using Ccs.Entities;
using Ccs.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ccs.Services
{
    /// <summary>
    /// The storage interface for <see cref="Contest"/>.
    /// </summary>
    /// <remarks>Note that the implementation for this interface shouldn't cache the result.</remarks>
    public interface IContestRepository
    {
        /// <summary>
        /// Finds the instance of contest.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <returns>The task for fetching entity.</returns>
        Task<ContestWrapper?> FindAsync(int cid);

        /// <summary>
        /// Updates the instance of contest.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="expression">The updating expression.</param>
        /// <returns>The task for updating.</returns>
        Task UpdateAsync(int cid, Expression<Func<Contest, Contest>> expression);

        /// <summary>
        /// Creates an instance of contest.
        /// </summary>
        /// <param name="kind">The contest kind.</param>
        /// <param name="user">The creation user.</param>
        /// <returns>The task for creating entity.</returns>
        Task<ContestWrapper> CreateAndAssignAsync(int kind, ClaimsPrincipal user);

        /// <summary>
        /// Lists the contests with original entity.
        /// </summary>
        /// <param name="page">The current page.</param>
        /// <param name="limit">The count per page.</param>
        /// <returns>The task for fetching paged lists.</returns>
        Task<IPagedList<ContestListModel>> ListAsync(int page = 1, int limit = 100);

        /// <summary>
        /// Finds the usage of some problem.
        /// </summary>
        /// <param name="probid">The problem ID.</param>
        /// <returns>The problem model.</returns>
        Task<List<Problem2Model>> FindProblemUsageAsync(int probid);

        /// <summary>
        /// Finds the participant information of some user.
        /// </summary>
        /// <param name="userid">The user ID.</param>
        /// <returns>The participant model.</returns>
        Task<List<ParticipantModel>> FindParticipantOfAsync(int userid);

        /// <summary>
        /// Check the problems.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The task for checking.</returns>
        Task<IEnumerable<(int, string)>> ListPermissionAsync(int userId);
    }
}
