using Ccs.Entities;
using Ccs.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ccs
{
    /// <summary>
    /// The context interface for fetching the information of a contest.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This context should be constructed by <see cref="Services.IContestContextFactory"/>.
    /// </para>
    /// </remarks>
    public partial interface IContestContext
    {
        /// <summary>
        /// The contest entity
        /// </summary>
        Contest Contest { get; }

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

        /// <summary>
        /// Get the auditlogs.
        /// </summary>
        /// <param name="page">The page to show.</param>
        /// <param name="pageCount">The count of pages to show.</param>
        /// <returns>The task with auditlogs.</returns>
        Task<IPagedList<SatelliteSite.Entities.Auditlog>> ViewLogsAsync(int page, int pageCount);

        /// <summary>
        /// Get the ajax update overview.
        /// </summary>
        /// <returns>The object for update results.</returns>
        Task<object> GetUpdatesAsync();
    }
}
