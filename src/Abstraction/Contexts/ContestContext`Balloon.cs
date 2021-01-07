using Ccs.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ccs
{
    public partial interface IContestContext
    {
        /// <summary>
        /// List the balloon and fill in problem related informations.
        /// </summary>
        /// <returns>The task for balloon models.</returns>
        Task<List<BalloonModel>> FetchBalloonsAsync();

        /// <summary>
        /// Set the balloon in contest as done.
        /// </summary>
        /// <param name="id">The balloon ID.</param>
        /// <returns>The task for setting done.</returns>
        Task SetBalloonDoneAsync(int id);
    }
}
