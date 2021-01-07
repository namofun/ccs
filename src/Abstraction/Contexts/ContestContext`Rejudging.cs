using Polygon.Entities;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs
{
    public partial interface IContestContext
    {
        /// <summary>
        /// Create an instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The created entity.</returns>
        Task<Rejudging> CreateRejudgingAsync(Rejudging entity);

        /// <summary>
        /// Update the instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The update task.</returns>
        Task UpdateRejudgingAsync(Rejudging entity);

        /// <summary>
        /// Update the instance of entity.
        /// </summary>
        /// <param name="id">The entity id.</param>
        /// <param name="expression">The update expression.</param>
        /// <returns>The update task.</returns>
        Task UpdateRejudgingAsync(int id, Expression<Func<Rejudging, Rejudging>> expression);

        /// <summary>
        /// Delete the instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The delete task.</returns>
        Task DeleteRejudgingAsync(Rejudging entity);

        /// <summary>
        /// Find the rejudging for certain contest.
        /// </summary>
        /// <param name="id">The rejudging ID.</param>
        /// <returns>The task for fetching rejudging entity.</returns>
        Task<Rejudging> FindRejudgingAsync(int id);

        /// <summary>
        /// List the rejudging for certain contest.
        /// </summary>
        /// <param name="includeStat">Whether to include statistics about undone rejudging progress.</param>
        /// <returns>The task for fetching rejudging entities.</returns>
        Task<List<Rejudging>> FetchRejudgingsAsync(bool includeStat = true);

        /// <summary>
        /// List the rejudging for certain contest.
        /// </summary>
        /// <returns>The task for fetching rejudging entities.</returns>
        Task<List<Judgehost>> FetchJudgehostsAsync();

        /// <summary>
        /// View the rejudging difference.
        /// </summary>
        /// <param name="rejudge">The rejudging entity.</param>
        /// <param name="filter">The fetching condition.</param>
        /// <returns>The task for fetching difference.</returns>
        Task<IEnumerable<RejudgingDifference>> ViewRejudgingAsync(
            Rejudging rejudge,
            Expression<Func<Judging, Judging, Submission, bool>>? filter = null);

        /// <summary>
        /// Rejudging several submissions with or without existing rejudging entity.
        /// </summary>
        /// <param name="predicate">The submissions to rejudge.</param>
        /// <param name="rejudge">The rejudging entity. If <c>null</c>, the submission won't be added to a rejudging.</param>
        /// <param name="fullTest">Whether to take a full test.</param>
        /// <returns>The task for batch rejudge submissions, returning the count of submissions being rejudged.</returns>
        Task<int> BatchRejudgeAsync(
            Expression<Func<Submission, Judging, bool>> predicate,
            Rejudging? rejudge = null,
            bool fullTest = false);

        /// <summary>
        /// Cancel the rejudging.
        /// </summary>
        /// <param name="rejudge">The rejudging entity.</param>
        /// <param name="uid">The operator user ID.</param>
        /// <returns>The task for cancelling.</returns>
        Task CancelRejudgingAsync(Rejudging rejudge, int uid);

        /// <summary>
        /// Apply the rejudging.
        /// </summary>
        /// <param name="rejudge">The rejudging entity.</param>
        /// <param name="uid">The operator user ID.</param>
        /// <returns>The task for applying.</returns>
        Task ApplyRejudgingAsync(Rejudging rejudge, int uid);
    }
}
