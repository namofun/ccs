using Polygon.Storages;

namespace Ccs.Services
{
    /// <summary>
    /// The facade for contest control system.
    /// </summary>
    public interface IContestFacade
    {
        /// <summary>
        /// The contest store
        /// </summary>
        IContestStore Contests { get; }

        /// <summary>
        /// The problem set store
        /// </summary>
        IProblemsetStore Problemset { get; }

        /// <summary>
        /// The team store
        /// </summary>
        ITeamStore Teams { get; }

        /// <summary>
        /// The submission store
        /// </summary>
        ISubmissionStore Submissions { get; }
    }
}
