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
        /// List the solutions satisfying some conditions.
        /// </summary>
        /// <typeparam name="T">The DTO entity.</typeparam>
        /// <param name="selector">The entity shaper.</param>
        /// <param name="predicate">The conditions.</param>
        /// <param name="limits">The count to take.</param>
        /// <returns>The task for fetching solutions.</returns>
        Task<List<T>> FetchSolutionsAsync<T>(
            Expression<Func<Submission, Judging, T>> selector,
            Expression<Func<Submission, bool>>? predicate = null,
            int? limits = null);

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
        /// Fetch the details pair.
        /// </summary>
        /// <remarks>Use left join so the judging run may be null.</remarks>
        /// <param name="problemId">The problem ID.</param>
        /// <param name="judgingId">The judging ID.</param>
        /// <returns>The task for fetching judging runs.</returns>
        Task<IEnumerable<(JudgingRun, Testcase)>> FetchDetailsAsync(int problemId, int judgingId);

        /// <summary>
        /// Fetch the details DTO.
        /// </summary>
        /// <remarks>Use inner join so the judging run is not null.</remarks>
        /// <typeparam name="T">The result DTO.</typeparam>
        /// <param name="selector">The result selector.</param>
        /// <param name="predicate">The condition.</param>
        /// <param name="limit">The count of selects.</param>
        /// <returns>The task for fetching judging runs.</returns>
        Task<IEnumerable<T>> FetchDetailsAsync<T>(
            Expression<Func<Testcase, JudgingRun, T>> selector,
            Expression<Func<Testcase, JudgingRun, bool>>? predicate = null,
            int? limit = null);

        /// <summary>
        /// Count the judgings with predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The count task.</returns>
        Task<int> CountJudgingAsync(
            Expression<Func<Judging, bool>> predicate);

        /// <summary>
        /// Find the judging for the id.
        /// </summary>
        /// <remarks>It is guaranteed that contest ID is correct.</remarks>
        /// <param name="id">The judging ID.</param>
        /// <returns>The task for finding judgement.</returns>
        Task<Judging> FindJudgingAsync(int id);

        /// <summary>
        /// List the judgings with predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="topCount">The top count.</param>
        /// <returns>The list task.</returns>
        Task<List<Judging>> FetchJudgingsAsync(Expression<Func<Judging, bool>> predicate, int topCount);

        /// <summary>
        /// Fetch the first submission source.
        /// </summary>
        /// <param name="predicate">The submission condition.</param>
        /// <returns>The task for fetching source code.</returns>
        Task<SubmissionSource> FetchSourceAsync(Expression<Func<Submission, bool>> predicate);

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
