using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xylab.Contesting.Models;
using Xylab.Polygon.Entities;

namespace Xylab.Contesting.Services
{
    /// <summary>
    /// Provides contract for submission controlling.
    /// </summary>
    public interface ISubmissionContext : IContestContext
    {
        /// <summary>
        /// Find the submission.
        /// </summary>
        /// <param name="submissionId">The submission ID.</param>
        /// <param name="includeJudgings">Whether to include judgings in results.</param>
        /// <returns>The task for fetching submission entity.</returns>
        Task<Submission?> FindSubmissionAsync(int submissionId, bool includeJudgings = false);

        /// <summary>
        /// Toggle the ignore status of submission.
        /// </summary>
        /// <param name="submission">The submission.</param>
        /// <param name="ignore">The new ignore status of submission.</param>
        /// <returns>The task for making ignore.</returns>
        Task ToggleIgnoreAsync(Submission submission, bool ignore);

        /// <summary>
        /// List the solutions satisfying some conditions.
        /// </summary>
        /// <typeparam name="T">The DTO entity.</typeparam>
        /// <param name="selector">The entity shaper.</param>
        /// <param name="predicate">The conditions.</param>
        /// <param name="limits">The count to take.</param>
        /// <returns>The task for fetching solutions.</returns>
        Task<List<T>> ListSolutionsAsync<T>(
            Expression<Func<Submission, Judging, T>> selector,
            Expression<Func<Submission, bool>>? predicate = null,
            int? limits = null);

        /// <summary>
        /// Fetch the solutions with given selector and predicate.
        /// </summary>
        /// <typeparam name="TSolution">The solution type.</typeparam>
        /// <param name="selector">The selector to shape model.</param>
        /// <param name="predicate">The predicate to filter model.</param>
        /// <param name="page">The current page.</param>
        /// <param name="perpage">The count per page.</param>
        /// <returns>The task for fetching solutions.</returns>
        Task<IPagedList<TSolution>> ListSolutionsAsync<TSolution>(
            Expression<Func<Submission, Judging, TSolution>> selector,
            Expression<Func<Submission, bool>> predicate,
            int page, int perpage);

        /// <summary>
        /// Fetch solution with contest.
        /// </summary>
        /// <typeparam name="TSolution">The solution type.</typeparam>
        /// <param name="selector">The result selector.</param>
        /// <param name="probid">The problem ID.</param>
        /// <param name="langid">The language ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <returns>The task for fetching solution list.</returns>
        Task<List<TSolution>> ListSolutionsAsync<TSolution>(
            Expression<Func<Submission, Judging, TSolution>> selector,
            int? probid = null, string? langid = null, int? teamid = null);

        /// <summary>
        /// Fetch solutions with contest.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="perPage">The count per page.</param>
        /// <param name="probid">The problem ID.</param>
        /// <param name="langid">The language ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <param name="verdict">The judging verdict.</param>
        /// <returns>The task for fetching solution list.</returns>
        Task<IPagedList<Xylab.Polygon.Models.Solution>> ListSolutionsAsync(
            int page, int perPage,
            int? probid = null, string? langid = null, int? teamid = null, Verdict? verdict = null);

        /// <summary>
        /// Fetch solutions with contest.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="perPage">The count per page.</param>
        /// <returns>The task for fetching solution list.</returns>
        Task<IPagedList<Xylab.Polygon.Models.Solution>> ListSolutionsAsync(int page, int perPage);

        /// <summary>
        /// Finds the specified solutions in this contest.
        /// </summary>
        /// <param name="submitid">The submission ID.</param>
        /// <returns>The task for fetching solution.</returns>
        Task<Xylab.Polygon.Models.Solution?> FindSolutionAsync(int submitid);

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
        Task<IEnumerable<(JudgingRun?, Testcase)>> GetDetailsAsync(int problemId, int judgingId);

        /// <summary>
        /// Fetch the detail DTO.
        /// </summary>
        /// <param name="problemId">The problem ID.</param>
        /// <param name="submitId">The submission ID.</param>
        /// <param name="judgingId">The judging ID.</param>
        /// <param name="runId">The judging run ID.</param>
        /// <returns>The task for fetching judging run.</returns>
        Task<JudgingRun?> GetDetailAsync(int problemId, int submitId, int judgingId, int runId);

        /// <summary>
        /// Fetch the details DTO.
        /// </summary>
        /// <remarks>Use inner join so the judging run is not null.</remarks>
        /// <typeparam name="T">The result DTO.</typeparam>
        /// <param name="selector">The result selector.</param>
        /// <param name="predicate">The condition.</param>
        /// <param name="limit">The count of selects.</param>
        /// <returns>The task for fetching judging runs.</returns>
        Task<IEnumerable<T>> GetDetailsAsync<T>(
            Expression<Func<Testcase, JudgingRun, T>> selector,
            Expression<Func<Testcase, JudgingRun, bool>>? predicate = null,
            int? limit = null);

        /// <summary>
        /// Count the judgings with predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The count task.</returns>
        Task<int> CountJudgingsAsync(Expression<Func<Judging, bool>> predicate);

        /// <summary>
        /// Find the judging for the id.
        /// </summary>
        /// <remarks>It is guaranteed that contest ID is correct.</remarks>
        /// <param name="id">The judging ID.</param>
        /// <returns>The task for finding judgement.</returns>
        Task<Judging?> FindJudgingAsync(int id);

        /// <summary>
        /// List the judgings with predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="topCount">The top count.</param>
        /// <returns>The list task.</returns>
        Task<List<Judging>> ListJudgingsAsync(Expression<Func<Judging, bool>> predicate, int topCount);

        /// <summary>
        /// List the judgings with predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The list task.</returns>
        /// <remarks>Should be used in event generating only.</remarks>
        Task<List<Judging>> ListJudgingsAsync(Expression<Func<Judging, bool>>? predicate = null);

        /// <summary>
        /// List the judging runs with predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>The list task.</returns>
        /// <remarks>Should be used in event generating only.</remarks>
        Task<List<JudgingRun>> ListJudgingRunsAsync(Expression<Func<JudgingRun, bool>>? predicate = null);

        /// <summary>
        /// Fetch the first submission source.
        /// </summary>
        /// <param name="predicate">The submission condition.</param>
        /// <returns>The task for fetching source code.</returns>
        Task<SubmissionSource?> GetSourceCodeAsync(Expression<Func<Submission, bool>> predicate);
    }
}
