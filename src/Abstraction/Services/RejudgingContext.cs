using Polygon.Entities;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    /// <summary>
    /// Provides contract for solution rejudging.
    /// </summary>
    public interface IRejudgingContext : IContestContext
    {
        /// <summary>
        /// Creates an instance of rejudging.
        /// </summary>
        /// <param name="rejudging">The rejudging.</param>
        /// <returns>The created rejudging.</returns>
        Task<Rejudging> CreateRejudgingAsync(Rejudging rejudging);

        /// <summary>
        /// Deletes the instance of rejudging.
        /// </summary>
        /// <param name="rejudging">The rejudging.</param>
        /// <returns>The delete task.</returns>
        Task DeleteRejudgingAsync(Rejudging rejudging);

        /// <summary>
        /// Finds the rejudging for certain contest.
        /// </summary>
        /// <param name="id">The rejudging ID.</param>
        /// <returns>The task for fetching rejudging entity.</returns>
        Task<Rejudging?> FindRejudgingAsync(int id);

        /// <summary>
        /// Lists the rejudging for certain contest.
        /// </summary>
        /// <param name="includeStat">Whether to include statistics about undone rejudging progress.</param>
        /// <returns>The task for fetching rejudging entities.</returns>
        Task<List<Rejudging>> FetchRejudgingsAsync(bool includeStat = true);

        /// <summary>
        /// Lists the rejudging for certain contest.
        /// </summary>
        /// <returns>The task for fetching rejudging entities.</returns>
        Task<List<Judgehost>> FetchJudgehostsAsync();

        /// <summary>
        /// Views the rejudging difference.
        /// </summary>
        /// <param name="rejudge">The rejudging entity.</param>
        /// <param name="filter">The fetching condition.</param>
        /// <returns>The task for fetching difference.</returns>
        Task<IEnumerable<RejudgingDifference>> ViewRejudgingAsync(Rejudging rejudge, Expression<Func<Judging, Judging, Submission, bool>>? filter = null);

        /// <summary>
        /// Rejudges several submissions with or without existing rejudging entity.
        /// </summary>
        /// <param name="predicate">The submissions to rejudge.</param>
        /// <param name="rejudge">The rejudging entity. If <c>null</c>, the submission won't be added to a rejudging.</param>
        /// <param name="fullTest">Whether to take a full test.</param>
        /// <returns>The task for batch rejudge submissions, returning the count of submissions being rejudged.</returns>
        Task<int> BatchRejudgeAsync(Expression<Func<Submission, Judging, bool>> predicate, Rejudging? rejudge = null, bool fullTest = false);

        /// <summary>
        /// Cancels the rejudging.
        /// </summary>
        /// <param name="rejudge">The rejudging entity.</param>
        /// <param name="uid">The operator user ID.</param>
        /// <returns>The task for cancelling.</returns>
        Task CancelRejudgingAsync(Rejudging rejudge, int uid);

        /// <summary>
        /// Applies the rejudging.
        /// </summary>
        /// <param name="rejudge">The rejudging entity.</param>
        /// <param name="uid">The operator user ID.</param>
        /// <returns>The task for applying.</returns>
        Task ApplyRejudgingAsync(Rejudging rejudge, int uid);
    }
}
