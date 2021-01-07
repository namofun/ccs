using Ccs.Entities;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace Ccs
{
    public partial interface IContestContext
    {
        /// <summary>
        /// Get the status of judge queue.
        /// </summary>
        /// <returns>The list of judge queues.</returns>
        Task<Polygon.Models.ServerStatus> GetJudgeQueueAsync();

        /// <summary>
        /// Find the submission.
        /// </summary>
        /// <param name="submissionId">The submission ID.</param>
        /// <param name="includeJudgings">Whether to include judgings in results.</param>
        /// <returns>The task for fetching submission entity.</returns>
        Task<Submission?> FindSubmissionAsync(int submissionId, bool includeJudgings = false);

        /// <summary>
        /// Fetch solutions with contest.
        /// </summary>
        /// <param name="probid">The problem ID.</param>
        /// <param name="langid">The language ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <param name="all">Whether to show all solutions.</param>
        /// <returns>The task for fetching solution list.</returns>
        Task<List<Polygon.Models.Solution>> FetchSolutionsAsync(
            int? probid = null, string? langid = null, int? teamid = null, bool all = false);

        /// <summary>
        /// Fetch solutions with contest.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="perPage">The count per page.</param>
        /// <returns>The task for fetching solution list.</returns>
        Task<IPagedList<Polygon.Models.Solution>> FetchSolutionsAsync(int page, int perPage);

        /// <summary>
        /// Fetch solutions with contest.
        /// </summary>
        /// <param name="submitid">The submission ID.</param>
        /// <returns>The task for fetching solution list.</returns>
        Task<Polygon.Models.Solution> FetchSolutionAsync(int submitid);

        /// <summary>
        /// Fetch solution with contest.
        /// </summary>
        /// <typeparam name="TSolution">The solution type.</typeparam>
        /// <param name="selector">The result selector.</param>
        /// <param name="probid">The problem ID.</param>
        /// <param name="langid">The language ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <returns>The task for fetching solution list.</returns>
        Task<List<TSolution>> FetchSolutionsAsync<TSolution>(
            Expression<Func<Submission, Judging, TSolution>> selector,
            int? probid = null, string? langid = null, int? teamid = null);

        /// <summary>
        /// Fetch solution with contest.
        /// </summary>
        /// <typeparam name="TSolution">The solution type.</typeparam>
        /// <param name="submitid">The submission ID.</param>
        /// <param name="selector">The result selector.</param>
        /// <returns>The task for fetching solution list.</returns>
        Task<TSolution> FetchSolutionAsync<TSolution>(
            int submitid,
            Expression<Func<Submission, Judging, TSolution>> selector);

        /// <summary>
        /// List the submissions.
        /// </summary>
        /// <typeparam name="T">The DTO entity.</typeparam>
        /// <param name="projection">The entity shaper.</param>
        /// <param name="predicate">The submission filter.</param>
        /// <returns>The submission list.</returns>
        Task<List<T>> ListSubmissionsAsync<T>(
            Expression<Func<Submission, T>> projection,
            Expression<Func<Submission, bool>>? predicate = null);

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
    }
}
