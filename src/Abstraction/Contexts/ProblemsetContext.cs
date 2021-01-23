using Ccs.Entities;
using Ccs.Models;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace Ccs
{
    /// <summary>
    /// The context interface for fetching the information of a contest.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This context implementation should be combined with <see cref="IContestContext"/>,
    /// which means any instance of <see cref="IContestContext"/> should be able to be casted to <see cref="IProblemsetContext"/>.
    /// </para>
    /// </remarks>
    public interface IProblemsetContext
    {
        /// <summary>
        /// The contest entity
        /// </summary>
        Contest Contest { get; }

        /// <summary>
        /// Lists the problems in the problemset.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="count">The count per page.</param>
        /// <returns>The task for fetching model.</returns>
        Task<IPagedList<ProblemModel>> ListProblemsetAsync(int page, int count);

        /// <summary>
        /// Finds the problem in current problemset.
        /// </summary>
        /// <param name="probid">The problem ID.</param>
        /// <param name="withStatement">Whether to include statement.</param>
        /// <returns>The task for fetching model.</returns>
        Task<ProblemModel?> FindProblemsetAsync(string probid, bool withStatement = false);

        /// <summary>
        /// Fetch the solutions with given selector and predicate.
        /// </summary>
        /// <typeparam name="TSolution">The solution type.</typeparam>
        /// <param name="selector">The selector to shape model.</param>
        /// <param name="predicate">The predicate to filter model.</param>
        /// <param name="page">The current page.</param>
        /// <param name="perpage">The count per page.</param>
        /// <returns>The task for fetching solutions.</returns>
        Task<IPagedList<TSolution>> FetchSolutionsAsync<TSolution>(
            Expression<Func<Submission, Judging, TSolution>> selector,
            Expression<Func<Submission, bool>> predicate,
            int page, int perpage);

        /// <summary>
        /// List the solutions satisfying some conditions.
        /// </summary>
        /// <typeparam name="TSolution">The DTO entity.</typeparam>
        /// <param name="selector">The entity shaper.</param>
        /// <param name="predicate">The conditions.</param>
        /// <param name="limits">The count to take.</param>
        /// <returns>The task for fetching solutions.</returns>
        Task<List<TSolution>> FetchSolutionsAsync<TSolution>(
            Expression<Func<Submission, Judging, TSolution>> selector,
            Expression<Func<Submission, bool>>? predicate = null,
            int? limits = null);

        /// <summary>
        /// Fetch the details pair.
        /// </summary>
        /// <remarks>Use left join so the judging run may be null.</remarks>
        /// <param name="problemId">The problem ID.</param>
        /// <param name="judgingId">The judging ID.</param>
        /// <returns>The task for fetching judging runs.</returns>
        Task<IEnumerable<(JudgingRun?, Testcase)>> FetchDetailsAsync(int problemId, int judgingId);

        /// <summary>
        /// Statistics the submission status of team.
        /// </summary>
        /// <param name="team">The team to discover.</param>
        /// <returns>The task for statistical result of submissions.</returns>
        Task<IReadOnlyDictionary<int, (int, int)>> StatisticsAsync(Team? team);

        /// <summary>
        /// Statistics the submission status of whole contest.
        /// </summary>
        /// <returns>The task for statistical result of submissions (Accepted, Total, AcceptedTeam, TotalTeam).</returns>
        Task<IReadOnlyDictionary<int, (int, int, int, int)>> StatisticsGlobalAsync();

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
        /// <param name="time">The submission time.</param>
        /// <returns>The task for creating submission.</returns>
        Task<Submission> SubmitAsync(
            string code,
            Language language,
            ContestProblem problem,
            Team team,
            IPAddress ipAddr,
            string via,
            string username,
            DateTimeOffset? time = null);

        /// <summary>
        /// Fetch the available languages.
        /// </summary>
        /// <returns>The task for fetching languages.</returns>
        Task<IReadOnlyList<Language>> FetchLanguagesAsync();
    }
}
