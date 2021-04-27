using Ccs.Entities;
using Ccs.Models;

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

        /// <summary>The jury level</summary>
        JuryLevel? JuryLevel { get; }

        /// <summary>Whether current user is a balloon runner</summary>
        bool IsBalloonRunner { get; }

        /// <summary>Whether current user is a jury</summary>
        bool IsJury { get; }

        /// <summary>Whether current user is an administrator</summary>
        bool IsAdministrator { get; }

        /// <summary>Whether current user belongs to a team</summary>
        bool HasTeam { get; }

        /// <summary>Whether status of team is accepted and no other restrictions are applied</summary>
        bool IsTeamAccepted { get; }

        /// <summary>Whether restrictions are failed</summary>
        bool IsRestrictionFailed { get; }

        /// <summary>The version of CCS</summary>
        string CcsVersion { get; }
    }
}
