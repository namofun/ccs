using Ccs.Entities;
using Ccs.Models;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    /// <summary>
    /// Provides contract for problemset.
    /// </summary>
    public interface IProblemsetContext : IContestContext
    {
        /// <summary>
        /// Lists the problems in the problemset.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="count">The count per page.</param>
        /// <param name="withDetail">Whether to include testcase count and scores.</param>
        /// <returns>The task for fetching model.</returns>
        Task<IPagedList<ProblemModel>> ListProblemsAsync(int page, int count, bool withDetail = false);

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
        /// List the solutions satisfying some conditions.
        /// </summary>
        /// <typeparam name="TSolution">The DTO entity.</typeparam>
        /// <param name="selector">The entity shaper.</param>
        /// <param name="predicate">The conditions.</param>
        /// <param name="limits">The count to take.</param>
        /// <returns>The task for fetching solutions.</returns>
        Task<List<TSolution>> ListSolutionsAsync<TSolution>(
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
        Task<IEnumerable<(JudgingRun?, Testcase)>> GetDetailsAsync(int problemId, int judgingId);

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
    }
}
