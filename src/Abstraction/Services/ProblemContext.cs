using Ccs.Entities;
using Ccs.Models;
using Microsoft.Extensions.FileProviders;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ccs
{
    public partial interface IContestContext
    {
        /// <summary>
        /// Lists the contest problems.
        /// </summary>
        /// <param name="nonCached">Whether to fetch the non-cached result.</param>
        /// <returns>The task for fetching problems.</returns>
        Task<ProblemCollection> ListProblemsAsync(bool nonCached = false);

        /// <summary>
        /// Finds the problem in current problemset.
        /// </summary>
        /// <param name="probid">The problem ID.</param>
        /// <param name="withStatement">Whether to include statement.</param>
        /// <returns>The task for fetching model.</returns>
        Task<ProblemModel?> FindProblemAsync(string probid, bool withStatement = false);

        /// <summary>
        /// Finds the problem in current problemset.
        /// </summary>
        /// <param name="probid">The problem ID.</param>
        /// <param name="withStatement">Whether to include statement.</param>
        /// <returns>The task for fetching model.</returns>
        Task<ProblemModel?> FindProblemAsync(int probid, bool withStatement = false);
    }
}

namespace Ccs.Services
{
    /// <summary>
    /// Provides contract for problem controlling.
    /// </summary>
    public interface IProblemContext : IContestContext
    {
        /// <summary>
        /// Creates problems by entity.
        /// </summary>
        /// <param name="entity">The contest problem to create.</param>
        /// <returns>The task for creating contest problems.</returns>
        Task CreateProblemAsync(ContestProblem entity);

        /// <summary>
        /// Updates problems by expression.
        /// </summary>
        /// <param name="origin">The original problem model.</param>
        /// <param name="expression">The expression for updating contest problem.</param>
        /// <returns>The task for updating contest problems.</returns>
        Task UpdateProblemAsync(ProblemModel origin, Expression<Func<ContestProblem, ContestProblem>> expression);

        /// <summary>
        /// Deletes such problem from contest.
        /// </summary>
        /// <param name="problem">The original problem model.</param>
        /// <returns>The task for deleting contest problems.</returns>
        Task DeleteProblemAsync(ProblemModel problem);

        /// <summary>
        /// Checks the availability of problems to add into contest.
        /// </summary>
        /// <param name="probId">The problem ID.</param>
        /// <param name="user">The current user.</param>
        /// <returns>The task for getting availability.</returns>
        Task<CheckResult> CheckProblemAvailabilityAsync(int probId, ClaimsPrincipal user);

        /// <summary>
        /// Gets the raw statements of current contest.
        /// </summary>
        /// <returns>The task for enlisting statements.</returns>
        Task<List<Statement>> FetchRawStatementsAsync();

        /// <summary>
        /// Gets the testcase of the <paramref name="problem"/>.
        /// </summary>
        /// <param name="problem">The problem ID.</param>
        /// <param name="testcaseId">The testcase ID.</param>
        /// <param name="filetype">The file type.</param>
        /// <returns>The task for getting file info.</returns>
        Task<IFileInfo?> FetchTestcaseAsync(ProblemModel problem, int testcaseId, string filetype);
    }
}
