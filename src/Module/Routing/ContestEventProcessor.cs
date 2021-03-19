using Ccs.Events;
using Ccs.Specifications;
using MediatR;
using Polygon.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public class ContestEventProcessor :
        INotificationHandler<ClarificationCreateEvent>,
        INotificationHandler<EventResetEvent>,
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

            await ctx.EmitCreateEventAsync(
                new Clarification(notification.Clarification, ctx.Contest.StartTime ?? DateTimeOffset.Now));
        }

        public async Task Handle(JudgingFinishedEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification, notification.ContestId);
            if (ctx == null) return;

            await ctx.EmitUpdateEventAsync(
                new Judgement(notification.Judging, ctx.Contest.StartTime ?? DateTimeOffset.Now));
        }

        public async Task Handle(JudgingBeginEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification, notification.ContestId);
            if (ctx == null) return;

            await ctx.EmitCreateEventAsync(
                new Judgement(notification.Judging, ctx.Contest.StartTime ?? DateTimeOffset.Now));
        }

        public async Task Handle(JudgingRunEmittedEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification, notification.ContestId);
            if (ctx == null) return;

            await ctx.EmitCreateEventAsync(
                notification.Runs,
                (r, i) => new Run(r, ctx.Contest.StartTime ?? DateTimeOffset.Now, i + notification.RankOfFirst));
        }

        public async Task Handle(SubmissionCreatedEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification, notification.Submission.ContestId);
            if (ctx == null) return;

            await ctx.EmitCreateEventAsync(
                new Submission(notification.Submission, ctx.Contest.StartTime ?? DateTimeOffset.Now));
        }

        public async Task Handle(EventResetEvent notification, CancellationToken cancellationToken)
        {
            var ctx = await TryGetContest(notification, notification.Contest.Id);
            if (ctx == null) return;

            // clean up
            await ((IJuryContext)ctx).CleanEventsAsync();

            var deadline = DateTimeOffset.Now;
            var contestTime = ctx.Contest.StartTime ?? deadline;

            // contests
            await ctx.EmitCreateEventAsync(new Contest(notification.Contest));

            // judgement-types
            await ctx.EmitCreateEventAsync(JudgementType.Defaults);

            // languages
            var langs = await ctx.ListLanguagesAsync();
            await ctx.EmitCreateEventAsync(langs, l => new Language(l));

            // groups
            var cats = await ctx.ListCategoriesAsync();
            await ctx.EmitCreateEventAsync(cats, c => new Group(c.Value));

            // organizations
            var affs = await ctx.ListAffiliationsAsync();
            await ctx.EmitCreateEventAsync(affs, a => new Organization(a.Value));

            // problems
            var probs = await ctx.ListProblemsAsync();
            await ctx.EmitCreateEventAsync(probs, p => new Problem(p));

            // teams
            var teams = await ((ITeamContext)ctx).ListTeamsAsync(t => t.Status == 1);
            await ctx.EmitCreateEventAsync(teams, t => new Team(t, affs[t.AffiliationId]));

            // clarifications
            var clars = await ((IClarificationContext)ctx).ListClarificationsAsync(c => c.SubmitTime <= deadline);
            await ctx.EmitCreateEventAsync(clars, c => new Clarification(c, contestTime));

            // submissions
            var submits = await ctx.ListSolutionsAsync(all: true);
            await ctx.EmitCreateEventAsync(submits, s => new Submission(s, contestTime));

            throw new NotImplementedException();
        }
    }
}
