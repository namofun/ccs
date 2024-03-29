﻿using System;
using Xylab.Contesting.Models;
using Xylab.Contesting.Services;

namespace Xylab.Contesting.Events
{
    public class ScoreboardRefreshEvent : IContextedNotification
    {
        public IContestInformation Contest { get; }

        public DateTimeOffset Deadline { get; }

        public DateTimeOffset StartTime { get; }

        public DateTimeOffset? FreezeTime { get; }

        public IContestContext Context { get; }

        public ScoreboardRefreshEvent(IContestContext context, DateTimeOffset? overrideEndTime = null)
        {
            Context = context;
            var contest = context.Contest;
            Contest = contest;
            var now = DateTimeOffset.Now;
            StartTime = Contest.StartTime ?? now;
            var endTime = overrideEndTime ?? (Contest.StartTime + Contest.EndTime) ?? now;
            if (now < endTime) endTime = now;
            Deadline = endTime;
            FreezeTime = Contest.StartTime + Contest.FreezeTime;
        }
    }
}
