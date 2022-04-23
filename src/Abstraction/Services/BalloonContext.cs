using System.Collections.Generic;
using System.Threading.Tasks;
using Xylab.Contesting.Models;

namespace Xylab.Contesting.Services
{
    /// <summary>
    /// Provides contract for balloon controlling.
    /// </summary>
    public interface IBalloonContext : IContestContext
    {
        /// <summary>
        /// Lists the balloon and fill in problem related informations.
        /// </summary>
        /// <returns>The task for balloon models.</returns>
        Task<List<BalloonModel>> ListAsync();

        /// <summary>
        /// Sets the balloon in contest as done.
        /// </summary>
        /// <param name="id">The balloon ID.</param>
        /// <returns>The task for setting done.</returns>
        Task HandleAsync(int id);
    }
}
