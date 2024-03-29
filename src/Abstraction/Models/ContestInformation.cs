﻿namespace Xylab.Contesting.Models
{
    /// <summary>
    /// The contest information.
    /// </summary>
    public interface IContestInformation : IContestTime
    {
        /// <summary>
        /// The contest ID
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The contest title
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The contest short name
        /// </summary>
        string ShortName { get; }

        /// <summary>
        /// Whether this contest is public
        /// </summary>
        bool IsPublic { get; }

        /// <summary>
        /// The ranking strategy
        /// </summary>
        /// <remarks>
        /// This represent the kind of ranking strategy.
        /// <list type="bullet"><c>0</c>: XCPC Rules</list>
        /// <list type="bullet"><c>1</c>: IOI Rules</list>
        /// <list type="bullet"><c>2</c>: Codeforces Rules</list>
        /// </remarks>
        int RankingStrategy { get; }

        /// <summary>
        /// The flags for contest kind
        /// </summary>
        /// <remarks>
        /// This represent the kind of contest.
        /// <list type="bullet"><c>0</c>: Normal contest</list>
        /// <list type="bullet"><c>1</c>: Practice contest</list>
        /// <list type="bullet"><c>2</c>: Problem set</list>
        /// </remarks>
        int Kind { get; }

        /// <summary>
        /// The flags for UI kind
        /// </summary>
        /// <remarks>
        /// The presentation defaults to the contest kind.
        /// <list type="bullet"><c>0</c>: DOMjudge UI and features</list>
        /// <list type="bullet"><c>1</c>: Codeforces UI and features</list>
        /// <list type="bullet"><c>2</c>: Legacy OJ problem set UI</list>
        /// When <see cref="Kind"/> is <c>0</c> and <see cref="IContestSettings.PreferGymUI"/> is set, this will be changed to <c>1</c>.
        /// </remarks>
        int Feature { get; }

        /// <summary>
        /// The contest settings
        /// </summary>
        IContestSettings Settings { get; }

        /// <summary>
        /// Checks whether the submission is available to current user.
        /// </summary>
        /// <param name="sameTeam">Whether current team is the same team as submission.</param>
        /// <param name="passProblem">Whether current team has passed the problem.</param>
        /// <returns>The available option.</returns>
        bool ShouldSubmissionAvailable(bool sameTeam, bool passProblem);

        /// <summary>
        /// Checks whether the scoreboard paging should be enabled.
        /// </summary>
        /// <returns>The value indicating that.</returns>
        bool ShouldScoreboardPaging();
    }
}
