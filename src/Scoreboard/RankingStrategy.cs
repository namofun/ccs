using Ccs.Entities;
using Ccs.Events;
using Ccs.Services;
using Polygon.Entities;
using Polygon.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ccs.Scoreboard
{
    /// <summary>
    /// The interface definition for ranking strategy.
    /// </summary>
    public interface IRankingStrategy
    {
        /// <summary>
        /// Sort the teams with correct rules.
        /// </summary>
        /// <param name="source">The source team scoreboard information.</param>
        /// <param name="contest">The contest entity.</param>
        /// <param name="isPublic">Whether to show the public scoreboard.</param>
        /// <returns>The enumerable for sorted scoreboard.</returns>
        IEnumerable<Team> SortByRule(IEnumerable<Team> source, Contest contest, bool isPublic);

        /// <summary>
        /// A submission is created now, and is pending for judgement.
        /// </summary>
        /// <param name="store">The scoreboard store.</param>
        /// <param name="contest">The contest entity.</param>
        /// <param name="args">The submission created event.</param>
        /// <returns>The task for updating scoreboard.</returns>
        Task Pending(IScoreboardStore store, Contest contest, SubmissionCreatedEvent args);

        /// <summary>
        /// A submission is judged as <see cref="Verdict.CompileError"/>.
        /// </summary>
        /// <param name="store">The scoreboard store.</param>
        /// <param name="contest">The contest entity.</param>
        /// <param name="args">The judging finished event.</param>
        /// <returns>The task for updating scoreboard.</returns>
        Task CompileError(IScoreboardStore store, Contest contest, JudgingFinishedEvent args);

        /// <summary>
        /// A submission is judged as <see cref="Verdict.WrongAnswer"/> or other rejected verdicts.
        /// </summary>
        /// <param name="store">The scoreboard store.</param>
        /// <param name="contest">The contest entity.</param>
        /// <param name="args">The judging finished event.</param>
        /// <returns>The task for updating scoreboard.</returns>
        Task Reject(IScoreboardStore store, Contest contest, JudgingFinishedEvent args);

        /// <summary>
        /// A submission is judged as <see cref="Verdict.Accepted"/>.
        /// </summary>
        /// <param name="store">The scoreboard store.</param>
        /// <param name="contest">The contest entity.</param>
        /// <param name="args">The judging finished event.</param>
        /// <returns>The task for updating scoreboard.</returns>
        Task Accept(IScoreboardStore store, Contest contest, JudgingFinishedEvent args);

        /// <summary>
        /// The scoreboard cache is asked to be refreshed.
        /// </summary>
        /// <param name="store">The scoreboard store.</param>
        /// <param name="args">The scoreboard refresh event.</param>
        /// <returns>The task for updating scoreboard.</returns>
        Task RefreshCache(IScoreboardStore store, ScoreboardRefreshEvent args);
    }
}
