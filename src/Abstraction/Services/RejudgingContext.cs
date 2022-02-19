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
        Task<Rejudging> CreateAsync(Rejudging rejudging);

        /// <summary>
        /// Deletes the instance of rejudging.
        /// </summary>
        /// <param name="rejudging">The rejudging.</param>
        /// <returns>The delete task.</returns>
        Task DeleteAsync(Rejudging rejudging);

        /// <summary>
        /// Finds the rejudging for certain contest.
        /// </summary>
        /// <param name="id">The rejudging ID.</param>
        /// <returns>The task for fetching rejudging entity.</returns>
        Task<Rejudging?> FindAsync(int id);

        /// <summary>
        /// Lists the rejudging for certain contest.
        /// </summary>
        /// <param name="includeStat">Whether to include statistics about undone rejudging progress.</param>
        /// <returns>The task for fetching rejudging entities.</returns>
        Task<List<Rejudging>> ListAsync(bool includeStat = true);

        /// <summary>
        /// Count the judgings with predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The count task.</returns>
        Task<int> CountJudgingsAsync(Expression<Func<Judging, bool>> predicate);

        /// <summary>
        /// Lists the rejudging for certain contest.
        /// </summary>
        /// <returns>The task for fetching rejudging entities.</returns>
        Task<List<Judgehost>> GetJudgehostsAsync();

        /// <summary>
        /// Views the rejudging difference.
        /// </summary>
        /// <param name="rejudge">The rejudging entity.</param>
        /// <param name="filter">The fetching condition.</param>
        /// <returns>The task for fetching difference.</returns>
        Task<IEnumerable<RejudgingDifference>> ViewAsync(Rejudging rejudge, Expression<Func<Judging, Judging, Submission, bool>>? filter = null);

        /// <summary>
        /// Rejudges several submissions with or without existing rejudging entity.
        /// </summary>
        /// <param name="predicate">The submissions to rejudge.</param>
        /// <param name="rejudge">The rejudging entity.</param>
        /// <param name="fullTest">Whether to take a full test.</param>
        /// <returns>The task for batch rejudge submissions, returning the count of submissions being rejudged.</returns>
        Task<int> RejudgeAsync(Expression<Func<Submission, Judging, bool>> predicate, Rejudging rejudge, bool fullTest = false);

        /// <summary>
        /// Cancels the rejudging.
        /// </summary>
        /// <param name="rejudge">The rejudging entity.</param>
        /// <param name="uid">The operator user ID.</param>
        /// <returns>The task for cancelling.</returns>
        Task CancelAsync(Rejudging rejudge, int uid);

        /// <summary>
        /// Applies the rejudging.
        /// </summary>
        /// <param name="rejudge">The rejudging entity.</param>
        /// <param name="uid">The operator user ID.</param>
        /// <returns>The task for applying.</returns>
        Task ApplyAsync(Rejudging rejudge, int uid);

        /// <summary>
        /// Starts a rejudging as system test.
        /// </summary>
        /// <param name="uid">The operating user ID.</param>
        /// <returns>The task for creating system test.</returns>
        Task<Models.CheckResult<Rejudging>> SystemTestAsync(int uid);

        /// <summary>
        /// Applies the rating changes.
        /// </summary>
        /// <returns>The task for result.</returns>
        Task ApplyRatingChangesAsync();

        /// <summary>
        /// Rolls back the rating changes.
        /// </summary>
        /// <returns>The task for result.</returns>
        Task RollbackRatingChangesAsync();
    }
}
