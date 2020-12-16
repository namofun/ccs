using Ccs.Entities;
using Ccs.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ccs.Services
{
    /// <summary>
    /// The storage interface for <see cref="Balloon"/>.
    /// </summary>
    /// <remarks>Note that all store interfaces shouldn't cache the result.</remarks>
    public interface IBalloonStore
    {
        /// <summary>
        /// Create a balloon for submission.
        /// </summary>
        /// <param name="submissionId">The submission ID.</param>
        /// <returns>The task for creating balloon.</returns>
        Task CreateAsync(int submissionId);

        /// <summary>
        /// List the balloon and fill in problem related informations.
        /// </summary>
        /// <param name="contest">The contest.</param>
        /// <param name="probs">The problem dictionary.</param>
        /// <returns>The task for balloon models.</returns>
        Task<List<BalloonModel>> ListAsync(Contest contest, Dictionary<int, ProblemModel> probs);

        /// <summary>
        /// Set the balloon in contest as done.
        /// </summary>
        /// <param name="contest">The contest.</param>
        /// <param name="id">The balloon ID.</param>
        /// <returns>The task for setting done.</returns>
        Task SetDoneAsync(Contest contest, int id);
    }
}
