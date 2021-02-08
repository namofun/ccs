using Ccs.Entities;

namespace Ccs.Services
{
    /// <summary>
    /// The accessor to contest informations.
    /// </summary>
    public interface IContestContextAccessor : IContestInformation
    {
        /// <summary>The context service</summary>
        IContestContext Context { get; }

        /// <summary>The current team</summary>
        Team? Team { get; }

        /// <summary>Whether current user is a jury</summary>
        bool IsJury { get; }

        /// <summary>Whether current user belongs to a team</summary>
        bool HasTeam { get; }
    }
}
