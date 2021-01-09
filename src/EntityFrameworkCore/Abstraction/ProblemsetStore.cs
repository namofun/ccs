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
    /// The storage interface for <see cref="ContestProblem"/>.
    /// </summary>
    /// <remarks>Note that all store interfaces shouldn't cache the result.</remarks>
    public interface IProblemsetStore
    {
        /// <summary>
        /// Create the contest problem.
        /// </summary>
        /// <param name="problem">The problem entity.</param>
        /// <returns>The task for creating.</returns>
        Task CreateAsync(ContestProblem problem);

        /// <summary>
        /// Update the contest problem.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="probid">The problem ID.</param>
        /// <param name="expression">The update expression.</param>
        /// <returns>The task for updating.</returns>
        Task UpdateAsync(int cid, int probid, Expression<Func<ContestProblem>> expression);

        /// <summary>
        /// Delete the problem from contest.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="probid">The problem ID.</param>
        /// <returns>The task for deleting.</returns>
        Task DeleteAsync(int cid, int probid);

        /// <summary>
        /// List the contest problem.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <returns>The task for listing problems.</returns>
        Task<IReadOnlyList<ProblemModel>> ListAsync(int cid);

        /// <summary>
        /// List contest problems by problem view.
        /// </summary>
        /// <param name="probid">The problem ID.</param>
        /// <returns>The contest problems.</returns>
        Task<IReadOnlyList<ProblemModel>> ListByProblemAsync(int probid);

        /// <summary>
        /// Get all statements from problems.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <returns>The task for fetching list of statements.</returns>
        Task<List<Problem>> RawProblemsAsync(int cid);

        /// <summary>
        /// Check the availability of problems to add into contest.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="probid">The problem ID.</param>
        /// <param name="user">The current user.</param>
        /// <returns>The task for getting availability.</returns>
        Task<CheckResult> CheckAvailabilityAsync(int cid, int probid, int? user);
    }
}
