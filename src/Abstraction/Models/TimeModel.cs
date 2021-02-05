using System;

namespace Ccs.Models
{
    /// <inheritdoc cref="IContestTime" />
    public class TimeOnlyModel : IContestTime
    {
        /// <inheritdoc />
        public DateTimeOffset? StartTime { get; set; }

        /// <inheritdoc />
        public TimeSpan? FreezeTime { get; set; }

        /// <inheritdoc />
        public TimeSpan? EndTime { get; set; }

        /// <inheritdoc />
        public TimeSpan? UnfreezeTime { get; set; }
    }
}
