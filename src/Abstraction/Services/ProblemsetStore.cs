using Ccs.Entities;
using Ccs.Models;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
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
        /// <param name="problem">The problem model.</param>
        /// <returns>The task for deleting.</returns>
        Task DeleteAsync(ProblemModel problem);

        /// <summary>
        /// List the contest problem.
        /// </summary>
        /// <param name="contest">The contest.</param>
        /// <returns>The task for listing problems.</returns>
        Task<IReadOnlyList<ProblemModel>> ListAsync(Contest contest);

        /// <summary>
        /// List contest problems by problem view.
        /// </summary>
        /// <param name="problem">The problem view.</param>
        /// <returns></returns>
        Task<List<ContestProblem>> ListByProblemAsync(Polygon.Entities.Problem problem);

        /*
        [Obsolete]
        Task<(bool ok, string msg)> CheckAvailabilityAsync(int cid, int pid, ClaimsPrincipal user);
        */
    }
}
