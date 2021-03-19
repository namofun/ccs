using Ccs.Events;
using MediatR;
using Polygon.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public class ContestEventProcessor :
        INotificationHandler<ClarificationCreateEvent>,
        INotificationHandler<JudgingFinishedEvent>,
        INotificationHandler<JudgingBeginEvent>,
        INotificationHandler<JudgingRunEmittedEvent>,
        INotificationHandler<SubmissionCreatedEvent>
    {
        public ScopedContestContextFactory Factory { get; }

        public ContestEventProcessor(ScopedContestContextFactory factory)
        {
            Factory = factory;
        }

        public async Task<IContestContext> TryGetContest(INotification notification, int? cid)
        {
            IContestContext ctx;
            if (notification is IContextedNotification contextedNotification)
            {
                ctx = contextedNotification.Context;
            }
            else
            {
                if (!cid.HasValue) return null;
                ctx = await Factory.CreateAsync(cid.Value);
            }

            return ctx.Contest.Kind == CcsDefaults.KindDom && ctx.Contest.Settings.EventAvailable ? ctx : null;
        }

        public async Task Handle(ClarificationCreateEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification, notification.Clarification.ContestId);
            if (ctx == null) return;

            var spec = new Specifications.Clarification(notification.Clarification, ctx.Contest.StartTime ?? DateTimeOffset.Now);
            await ctx.EmitEventAsync(spec, "create");
        }

        public async Task Handle(JudgingFinishedEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification, notification.ContestId);
            if (ctx == null) return;

            var spec = new Specifications.Judgement(notification.Judging, ctx.Contest.StartTime ?? DateTimeOffset.Now);
            await ctx.EmitEventAsync(spec, "update");
        }

        public async Task Handle(JudgingBeginEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification, notification.ContestId);
            if (ctx == null) return;

            var spec = new Specifications.Judgement(notification.Judging, ctx.Contest.StartTime ?? DateTimeOffset.Now);
            await ctx.EmitEventAsync(spec, "create");
        }

        public async Task Handle(JudgingRunEmittedEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification, notification.ContestId);
            if (ctx == null) return;

            for (int i = 0; i < notification.Runs.Count; i++)
            {
                var spec = new Specifications.Run(notification.Runs[i], ctx.Contest.StartTime ?? DateTimeOffset.Now, i + notification.RankOfFirst);
                await ctx.EmitEventAsync(spec, "create");
            }
        }

        public async Task Handle(SubmissionCreatedEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification, notification.Submission.ContestId);
            if (ctx == null) return;

            var spec = new Specifications.Submission(notification.Submission, ctx.Contest.StartTime ?? DateTimeOffset.Now);
            await ctx.EmitEventAsync(spec, "create");
        }
    }
}
