using Ccs.Models;
using Ccs.Services;
using System;

namespace Ccs.Events
{
    public class ScoreboardRefreshEvent : IContextedNotification
    {
        public IContestInformation Contest { get; }

        public DateTimeOffset Deadline { get; }

        public DateTimeOffset StartTime { get; }

        public DateTimeOffset? FreezeTime { get; }

        public IContestContext Context { get; }

        public ScoreboardRefreshEvent(IContestContext context)
        {
            Context = context;
            var contest = context.Contest;
            Contest = contest;
            var now = DateTimeOffset.Now;
            StartTime = Contest.StartTime ?? now;
            var endTime = (Contest.StartTime + Contest.EndTime) ?? now;
            if (now < endTime) endTime = now;
            Deadline = endTime;
            FreezeTime = Contest.StartTime + Contest.FreezeTime;
        }
    }
}
