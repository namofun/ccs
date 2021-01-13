using Ccs.Entities;
using System;

namespace Ccs
{
    /// <summary>
    /// The accessor to contest informations.
    /// </summary>
    public interface IContestContextAccessor
    {
        /// <summary>The context service</summary>
        IContestContext Context { get; }

        /// <summary>The problem collection</summary>
        ProblemCollection Problems { get; }

        /// <summary>The current team</summary>
        Team? Team { get; }

        /// <summary>Whether current user is a jury</summary>
        bool IsJury { get; }

        /// <summary>Whether current user belongs to a team</summary>
        bool HasTeam { get; }

        /// <inheritdoc cref="Contest.Id" />
        int Id { get; }

        /// <inheritdoc cref="Contest.Name" />
        string Name { get; }

        /// <inheritdoc cref="Contest.ShortName" />
        string ShortName { get; }

        /// <inheritdoc cref="Contest.StartTime" />
        DateTimeOffset? StartTime { get; }

        /// <inheritdoc cref="Contest.FreezeTime" />
        TimeSpan? FreezeTime { get; }

        /// <inheritdoc cref="Contest.EndTime" />
        TimeSpan? EndTime { get; }

        /// <inheritdoc cref="Contest.UnfreezeTime" />
        TimeSpan? UnfreezeTime { get; }

        /// <inheritdoc cref="Contest.RankingStrategy" />
        int RankingStrategy { get; }

        /// <inheritdoc cref="Contest.IsPublic" />
        bool IsPublic { get; }

        /// <inheritdoc cref="Contest.PrintingAvailable" />
        bool PrintingAvailable { get; }

        /// <inheritdoc cref="Contest.BalloonAvailable" />
        bool BalloonAvailable { get; }

        /// <inheritdoc cref="Contest.RegisterCategory" />
        int? RegisterCategory { get; }

        /// <inheritdoc cref="Contest.Kind" />
        int Kind { get; }

        /// <inheritdoc cref="Contest.StatusAvailable" />
        int StatusAvailable { get; }

        /// <inheritdoc cref="Contest.GetState(DateTimeOffset?)" />
        ContestState GetState(DateTimeOffset? nowTime = null);
    }
}
