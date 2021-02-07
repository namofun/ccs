using Ccs.Entities;
using Ccs.Models;
using Microsoft.Extensions.FileProviders;
using Polygon.Entities;
using Polygon.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ccs.Services
{
    /// <summary>
    /// Provides contract for gym.
    /// </summary>
    public interface IGymContext : IContestContext
    {
        /// <summary>
        /// Fetch the details pair.
        /// </summary>
        /// <remarks>Use left join so the judging run may be null.</remarks>
        /// <param name="problemId">The problem ID.</param>
        /// <param name="judgingId">The judging ID.</param>
        /// <returns>The task for fetching judging runs.</returns>
        Task<IEnumerable<(JudgingRun?, Testcase)>> FetchDetailsAsync(int problemId, int judgingId);

        /// <summary>
        /// Fetch solutions with contest.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="perPage">The count per page.</param>
        /// <returns>The task for fetching solution list.</returns>
        Task<IPagedList<Solution>> FetchSolutionsAsync(int page, int perPage);

        /// <summary>
        /// Gets the testcase of the <paramref name="problem"/>.
        /// </summary>
        /// <param name="problem">The problem ID.</param>
        /// <param name="testcaseId">The testcase ID.</param>
        /// <param name="filetype">The file type.</param>
        /// <returns>The task for getting file info.</returns>
        Task<IFileInfo?> FetchTestcaseAsync(ProblemModel problem, int testcaseId, string filetype);

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
