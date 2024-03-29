﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xylab.Contesting.Entities;
using Xylab.Contesting.Models;

namespace Xylab.Contesting.Services
{
    /// <summary>
    /// The interface for updating scoreboard.
    /// </summary>
    public interface IScoreboard
    {
        /// <summary>
        /// Creates a balloon for the submission.
        /// </summary>
        /// <param name="id">The submission ID.</param>
        /// <returns>The task for creating balloon.</returns>
        Task CreateBalloonAsync(int id);

        /// <summary>
        /// Removes all caches from the contest and set up the new ranks and scores.
        /// </summary>
        /// <param name="data">The scoreboard data.</param>
        /// <returns>The task for refreshing scoreboard cells.</returns>
        Task RefreshAsync(ScoreboardRawData data);

        /// <summary>
        /// Updates the score cache, or insert the default value.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <param name="probid">The problem ID.</param>
        /// <param name="insert">The insert expression.</param>
        /// <param name="update">The update expression.</param>
        /// <returns>The task for updating the score cache.</returns>
        Task ScoreUpsertAsync(
            int cid, int teamid, int probid,
            Expression<Func<ScoreCache>> insert,
            Expression<Func<ScoreCache, ScoreCache>> update);

        /// <summary>
        /// Updates the score cache value.
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
        /// Updates the score cache value with condition.
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
        /// Checks whether the current solution is first to solve.
        /// </summary>
        /// <remarks>Note that this should be executed before running exact updates. This only runs when we are using the XCPC rules.</remarks>
        /// <param name="cid">The contest ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <param name="probid">The problem ID.</param>
        /// <returns>Whether this solution is first to solve.</returns>
        Task<bool> IsFirstToSolveAsync(int cid, int teamid, int probid);

        /// <summary>
        /// Updates the rank cache value, or insert the default value.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <param name="probid">The problem ID.</param>
        /// <param name="insert">The insert expression linked with score cache.</param>
        /// <param name="update">The update expression linked with excluded entity.</param>
        /// <returns>The task for updating the rank cache.</returns>
        Task RankUpsertAsync(
            int cid, int teamid, int probid,
            Expression<Func<ScoreCache, RankCache>> insert,
            Expression<Func<RankCache, RankCache, RankCache>> update);

        /// <summary>
        /// Gets the solution information for recalculating scoreboard.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="deadline">The latest submission time.</param>
        /// <returns>The task for fetching solutions.</returns>
        Task<List<ScoreCalculateModel>> FetchSolutionsAsync(int cid, DateTimeOffset deadline);

        /// <summary>
        /// Rebuilds the partial score used by some contest.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <returns>The task for recalculating.</returns>
        Task RebuildPartialScoreAsync(int cid);

        /// <summary>
        /// Rebuilds the submission statistics for some contest.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <returns>The task for recalculating.</returns>
        Task RebuildStatisticsAsync(int cid);

        /// <summary>
        /// Emits an event.
        /// </summary>
        /// <param name="event">The new event entity.</param>
        /// <returns>The task for emitting.</returns>
        Task EmitEventAsync(Event @event);

        /// <summary>
        /// Gets the scores in the contest.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <returns>The task for fetching.</returns>
        Task<IReadOnlyDictionary<int, int>> GetModeScoresAsync(int cid);
    }
}
