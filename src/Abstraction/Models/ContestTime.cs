using System;

namespace Xylab.Contesting.Models
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

        /// <summary>
        /// Gets the state of contest.
        /// </summary>
        /// <param name="nowTime">The current datetime.</param>
        /// <returns>The state of contest.</returns>
        Entities.ContestState GetState(DateTimeOffset? nowTime = null);
    }
}
