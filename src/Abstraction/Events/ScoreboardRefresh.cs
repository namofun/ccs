﻿using Ccs.Entities;
using MediatR;
using System;

namespace Ccs.Events
{
    /// <summary>
    /// The event for a scoreboard refresh request.
    /// </summary>
    public class ScoreboardRefreshEvent : INotification
    {
        /// <summary>
        /// The contest
        /// </summary>
        public IContestInformation Contest { get; }

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
        public DateTimeOffset? FreezeTime { get; }

        /// <summary>
        /// The contest problems
        /// </summary>
        public ProblemCollection Problems { get; }

        /// <summary>
        /// Construct a <see cref="ScoreboardRefreshEvent"/>.
        /// </summary>
        /// <param name="contest">The contest entity.</param>
        /// <param name="problems">The contest problems.</param>
        public ScoreboardRefreshEvent(Contest contest, ProblemCollection problems)
        {
            Contest = contest;
            var now = DateTimeOffset.Now;
            StartTime = Contest.StartTime ?? now;
            var endTime = (Contest.StartTime + Contest.EndTime) ?? now;
            if (now < endTime) endTime = now;
            Deadline = endTime;
            Problems = problems;
            FreezeTime = Contest.StartTime + Contest.FreezeTime;
        }
    }
}
