using Ccs.Entities;
using Ccs.Models;
using Polygon.Entities;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Ccs.Contexts
{
    /// <summary>
    /// The context interface for fetching the information of a contest.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This context should be constructed by <see cref="IContestContextFactory"/>.
    /// </para>
    /// </remarks>
    public interface IContestContext
    {
        /// <summary>
        /// The contest entity
        /// </summary>
        Contest Contest { get; }

        /// <summary>
        /// Fetch the contest scoreboard.
        /// </summary>
        /// <returns>The task for fetching scoreboard model.</returns>
        Task<ScoreboardModel> FetchScoreboardAsync();

        /// <summary>
        /// Fetch the available languages.
        /// </summary>
        /// <returns>The task for fetching languages.</returns>
        Task<IReadOnlyList<Language>> FetchLanguagesAsync();

        /// <summary>
        /// Fetch the contest problems.
        /// </summary>
        /// <returns>The task for fetching problems.</returns>
        Task<IReadOnlyList<ProblemModel>> FetchProblemsAsync();

        /// <summary>
        /// Find team by team ID.
        /// </summary>
        /// <param name="teamId">The team ID.</param>
        /// <returns>The task for fetching team entity.</returns>
        Task<Team?> FindTeamByIdAsync(int teamId);

        /// <summary>
        /// Find team by user ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The task for fetching team entity.</returns>
        Task<Team?> FindTeamByUserAsync(int userId);

        /// <summary>
        /// Create a submission for team.
        /// </summary>
        /// <param name="code">The source code.</param>
        /// <param name="language">The language.</param>
        /// <param name="problem">The problem.</param>
        /// <param name="team">The team.</param>
        /// <param name="ipAddr">The IP Address.</param>
        /// <param name="via">The submission source.</param>
        /// <param name="username">The submission username.</param>
        /// <returns>The task for creating submission.</returns>
        Task<Submission> SubmitAsync(
            string code,
            Language language,
            ContestProblem problem,
            Team team,
            IPAddress ipAddr,
            string via,
            string username);
    }
}
