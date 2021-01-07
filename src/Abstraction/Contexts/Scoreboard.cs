using Ccs.Entities;
using Ccs.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    /// <summary>
    /// The interface for updating scoreboard.
    /// </summary>
    public interface IScoreboard
    {
        /// <summary>
        /// Remove all caches from the contest and set up the new ranks and scores.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="ranks">The new rank entities.</param>
        /// <param name="scores">The new score entities.</param>
        /// <returns>The task for refreshing scoreboard cells.</returns>
        Task RefreshAsync(
            int cid,
            IEnumerable<RankCache> ranks,
            IEnumerable<ScoreCache> scores);

        /// <summary>
        /// Update the score cache, or insert the default value.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <param name="probid">The problem ID.</param>
        /// <param name="expression">The update expression. If this is insert mode, fields from source will be downgraded as default value of that type.</param>
        /// <returns>The task for updating the score cache.</returns>
        Task ScoreUpsertAsync(
            int cid, int teamid, int probid,
            Expression<Func<ScoreCache, ScoreCache>> expression);

        /// <summary>
        /// Update the score cache value.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <param name="probid">The problem ID.</param>
        /// <param name="expression">The update expression.</param>
        /// <returns>The task for updating the score cache.</returns>
        Task ScoreUpdateAsync(
            int cid, int teamid, int probid,
            Expression<Func<ScoreCache, ScoreCache>> expression);

        /// <summary>
        /// Update the score cache value with condition.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <param name="probid">The problem ID.</param>
        /// <param name="predicate">The update condition predicate.</param>
        /// <param name="expression">The update expression.</param>
        /// <returns>The task for updating the score cache, with the result indicating whether the condition hits.</returns>
        Task<bool> ScoreUpdateAsync(
            int cid, int teamid, int probid,
            Expression<Func<ScoreCache, bool>> predicate,
            Expression<Func<ScoreCache, ScoreCache>> expression);

        /// <summary>
        /// Check whether the current solution is first to solve.
        /// </summary>
        /// <remarks>Note that this should be executed before running exact updates. This only runs when we are using the XCPC rules.</remarks>
        /// <param name="cid">The contest ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <param name="probid">The problem ID.</param>
        /// <returns>Whether this solution is first to solve.</returns>
        Task<bool> IsFirstToSolveAsync(int cid, int teamid, int probid);

        /// <summary>
        /// Update the rank cache value, or insert the default value.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <param name="probid">The problem ID.</param>
        /// <param name="expression">The update expression linked with score cache.</param>
        /// <returns>The task for updating the rank cache.</returns>
        Task RankUpsertAsync(
            int cid, int teamid, int probid,
            Expression<Func<RankCache, ScoreCache, RankCache>> expression);

        /// <summary>
        /// Fetch the solution information for recalculating scoreboard.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="deadline">The latest submission time.</param>
        /// <returns>The task for fetching solutions.</returns>
        Task<List<ScoreCalculateModel>> FetchRecalculateAsync(int cid, DateTimeOffset deadline);

        /// <summary>
        /// Rebuild the partial score used by some contest.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <returns>The task for recalculating.</returns>
        Task RebuildPartialScoreAsync(int cid);
    }
}
