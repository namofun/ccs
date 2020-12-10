using Ccs.Entities;
using MediatR;
using System;

namespace Ccs.Events
{
    public class ScoreboardRefreshEvent : INotification
    {
        /// <summary>
        /// The contest
        /// </summary>
        public Contest Contest { get; }

        /// <summary>
        /// The contest end time or current request time
        /// </summary>
        public DateTimeOffset Deadline { get; }

        /// <summary>
        /// The contest start time
        /// </summary>
        public DateTimeOffset StartTime { get; }

        /// <summary>
        /// The contest freeze time
        /// </summary>
        public DateTimeOffset? FreezeTime => Contest.StartTime + Contest.FreezeTime;

        /// <summary>
        /// Construct a <see cref="ScoreboardRefreshEvent"/>.
        /// </summary>
        /// <param name="contest">The contest entity.</param>
        public ScoreboardRefreshEvent(Contest contest)
        {
            Contest = contest;
            var now = DateTimeOffset.Now;
            StartTime = Contest.StartTime ?? now;
            var endTime = (Contest.StartTime + Contest.EndTime) ?? now;
            if (now < endTime) endTime = now;
            Deadline = endTime;
        }
    }
}
