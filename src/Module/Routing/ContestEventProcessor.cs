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
        INotificationHandler<JudgingRunEmitted>,
        INotificationHandler<SubmissionCreatedEvent>
    {
        public ScopedContestContextFactory Factory { get; }

        public ContestEventProcessor(ScopedContestContextFactory factory)
        {
            Factory = factory;
        }

        public async Task<IContestContext> TryGetContest(int? cid)
        {
            if (!cid.HasValue) return null;
            var ctx = await Factory.CreateAsync(cid.Value);
            return ctx.Contest.Kind == CcsDefaults.KindDom ? ctx : null;
        }

        public Task Handle(ClarificationCreateEvent notification, CancellationToken cancellationToken)
        {
            if (notification.Contest.Contest.Kind != CcsDefaults.KindDom) return Task.CompletedTask;
            var spec = new Specifications.Clarification(notification.Clarification, notification.Contest.Contest.StartTime ?? DateTimeOffset.Now);
            return notification.Contest.EmitEventAsync(spec, "create");
        }

        public async Task Handle(JudgingFinishedEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification.ContestId);
            if (ctx == null) return;

            var spec = new Specifications.Judgement(notification.Judging, ctx.Contest.StartTime ?? DateTimeOffset.Now);
            await ctx.EmitEventAsync(spec, "update");
        }

        public async Task Handle(JudgingBeginEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification.ContestId);
            if (ctx == null) return;

            var spec = new Specifications.Judgement(notification.Judging, ctx.Contest.StartTime ?? DateTimeOffset.Now);
            await ctx.EmitEventAsync(spec, "create");
        }

        public async Task Handle(JudgingRunEmitted notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification.ContestId);
            if (ctx == null) return;

            for (int i = 0; i < notification.Runs.Count; i++)
            {
                var spec = new Specifications.Run(notification.Runs[i], ctx.Contest.StartTime ?? DateTimeOffset.Now, i + notification.RankOfFirst);
                await ctx.EmitEventAsync(spec, "create");
            }
        }

        public async Task Handle(SubmissionCreatedEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification.Submission.ContestId);
            if (ctx == null) return;

            var spec = new Specifications.Submission(notification.Submission, ctx.Contest.StartTime ?? DateTimeOffset.Now);
            await ctx.EmitEventAsync(spec, "create");
        }
    }
}
