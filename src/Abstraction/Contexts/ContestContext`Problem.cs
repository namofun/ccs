using Ccs.Entities;
using Ccs.Models;
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
        /// Fetch the contest problems.
        /// </summary>
        /// <param name="nonCached">Whether to fetch the non-cached result.</param>
        /// <returns>The task for fetching problems.</returns>
        Task<ProblemCollection> FetchProblemsAsync(bool nonCached = false);

        /// <summary>
        /// Create problems by expression.
        /// </summary>
        /// <param name="entity">The contest problem to create.</param>
        /// <returns>The task for creating contest problems.</returns>
        Task CreateProblemAsync(ContestProblem entity);

        /// <summary>
        /// Update problems by expression.
        /// </summary>
        /// <param name="origin">The original problem model.</param>
        /// <param name="expression">The expression for updating contest problem.</param>
        /// <returns>The task for updating contest problems.</returns>
        Task UpdateProblemAsync(ProblemModel origin, Expression<Func<ContestProblem, ContestProblem>> expression);

        /// <summary>
        /// Delete such problem from contest.
        /// </summary>
        /// <param name="problem">The original problem model.</param>
        /// <returns>The task for deleting contest problems.</returns>
        Task DeleteProblemAsync(ProblemModel problem);

        /// <summary>
        /// Check the availability of problems to add into contest.
        /// </summary>
        /// <param name="probId">The problem ID.</param>
        /// <param name="user">The current user.</param>
        /// <returns>The task for getting availability.</returns>
        Task<CheckResult> CheckProblemAvailabilityAsync(int probId, ClaimsPrincipal user);

        /// <summary>
        /// Fetch the raw statements of current contest.
        /// </summary>
        /// <returns>The task for enlisting statements.</returns>
        Task<List<Statement>> FetchRawStatementsAsync();
    }
}
