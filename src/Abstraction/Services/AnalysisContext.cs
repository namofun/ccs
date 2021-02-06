using Ccs.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ccs.Services
{
    /// <summary>
    /// Provides contract for contest analysis.
    /// </summary>
    public interface IAnalysisContext : IContestContext
    {
        /// <summary>
        /// Fetch the team names as a lookup dictionary.
        /// </summary>
        /// <returns>The task for getting this dictionary.</returns>
        Task<IReadOnlyDictionary<int, Team>> FetchTeamsAsync();
    }
}
