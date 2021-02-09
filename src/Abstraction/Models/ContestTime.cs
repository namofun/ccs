using System;

namespace Ccs.Models
{
    /// <summary>
    /// The contest time.
    /// </summary>
    public interface IContestTime
    {
        /// <summary>
        /// The start time
        /// </summary>
        DateTimeOffset? StartTime { get; }

        /// <summary>
        /// The freeze time
        /// </summary>
        TimeSpan? FreezeTime { get; }

        /// <summary>
        /// The end time
        /// </summary>
        TimeSpan? EndTime { get; }

        /// <summary>
        /// The unfreeze time
        /// </summary>
        TimeSpan? UnfreezeTime { get; }
    }
}
