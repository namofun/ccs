using Ccs.Events;
using Ccs.Specifications;
using MediatR;
using Polygon.Events;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public class ContestEventProcessor :
        INotificationHandler<ClarificationCreateEvent>,
        INotificationHandler<JudgingFinishedEvent>,
        INotificationHandler<JudgingBeginEvent>,
        INotificationHandler<JudgingRunEmittedEvent>,
        INotificationHandler<RejudgingAppliedEvent>,
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

            if (ctx == null) return null;
            if (ctx.Contest.Kind == CcsDefaults.KindDom && ctx.Contest.Settings.EventAvailable) return ctx;
            return null;
        }

        public async Task Handle(ClarificationCreateEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification, notification.Clarification.ContestId);
            if (ctx == null) return;

            await ctx.EmitEventAsync(
                new Clarification(notification.Clarification, ctx.Contest.StartTime ?? DateTimeOffset.Now));
        }

        public async Task Handle(JudgingFinishedEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification, notification.ContestId);
            if (ctx == null || !notification.Judging.Active) return;

            await ctx.EmitEventAsync(
                new Judgement(notification.Judging, ctx.Contest.StartTime ?? DateTimeOffset.Now),
                "update");
        }

        public async Task Handle(JudgingBeginEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification, notification.ContestId);
            if (ctx == null || !notification.Judging.Active) return;

            await ctx.EmitEventAsync(
                new Judgement(notification.Judging, ctx.Contest.StartTime ?? DateTimeOffset.Now));
        }

        public async Task Handle(JudgingRunEmittedEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification, notification.ContestId);
            if (ctx == null || !notification.Judging.Active) return;

            for (int i = 0; i < notification.Runs.Count; i++)
            {
                await ctx.EmitEventAsync(new Run(notification.Runs[i], ctx.Contest.StartTime ?? DateTimeOffset.Now, i + notification.RankOfFirst));
            }
        }

        public async Task Handle(SubmissionCreatedEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification, notification.Submission.ContestId);
            if (ctx == null) return;

            await ctx.EmitEventAsync(
                new Submission(notification.Submission, ctx.Contest.StartTime ?? DateTimeOffset.Now));
        }

        public async Task Handle(RejudgingAppliedEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification, notification.Rejudging.ContestId);
            if (ctx == null) return;

            var rejudging = notification.Rejudging;
            var results = await ((IRejudgingContext)ctx).ViewAsync(rejudging);
            var now = DateTimeOffset.Now;
            var contestTime = ctx.Contest.StartTime.Value;

            using var batch = new Models.EventBatch(ctx.Contest.Id, now, null);
            batch.AddCreate(results.Select(r => r.NewJudging), j => new Judgement(j, contestTime, null, now));

            await ctx.EnsureLastStateAsync();
            await ctx.EmitEventAsync(batch);
        }
    }
}
