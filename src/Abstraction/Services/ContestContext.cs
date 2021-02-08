using Ccs.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ccs.Services
{
    /// <summary>
    /// The context interface for fetching the information of a contest.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This context should be constructed by <see cref="IContestContextFactory"/>.
    /// </para>
    /// </remarks>
    public partial interface IContestContext
    {
        /// <summary>
        /// The contest entity
        /// </summary>
        IContestInformation Contest { get; }

        /// <summary>
        /// Fetch the contest scoreboard.
        /// </summary>
        /// <returns>The task for fetching scoreboard model.</returns>
        Task<ScoreboardModel> FetchScoreboardAsync();

        /// <summary>
        /// Fetch the available languages.
        /// </summary>
        /// <returns>The task for fetching languages.</returns>
        Task<IReadOnlyList<Polygon.Entities.Language>> FetchLanguagesAsync();
    }
}
