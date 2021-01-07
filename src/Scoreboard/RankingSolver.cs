using Ccs.Contexts;
using Ccs.Entities;
using Ccs.Events;
using Ccs.Models;
using Ccs.Services;
using MediatR;
using Polygon.Entities;
using Polygon.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ccs.Scoreboard
{
    /// <summary>
    /// The solver for ranking.
    /// </summary>
    public class RankingSolver :
        INotificationHandler<JudgingFinishedEvent>,
        INotificationHandler<SubmissionCreatedEvent>,
        INotificationHandler<ScoreboardRefreshEvent>,
        IRequestHandler<ScoreboardSortEvent, IEnumerable<IScoreboardRow>>
    {
        /// <summary>
        /// The interface for scoreboard storage
        /// </summary>
        private IScoreboard Store { get; }

        /// <summary>
        /// The interface for contest context
        /// </summary>
        private ScopedContestContextFactory Factory { get; }

        /// <summary>
        /// The known strategies
        /// </summary>
        public static readonly IReadOnlyList<IRankingStrategy> Strategies = new IRankingStrategy[]
        {
            new Query.XCPCRank(),
            new Query.OIRank(),
            new Query.CFRank(),
        };

        /// <summary>
        /// Choose the strategy for contest.
        /// </summary>
        /// <param name="contest">The contest entity.</param>
        /// <returns>The ranking strategy.</returns>
        private IRankingStrategy Select(Contest contest)
        {
            if (contest == null ||
                contest.RankingStrategy > Strategies.Count ||
                contest.RankingStrategy < 0)
                throw new ArgumentOutOfRangeException(nameof(contest));
            return Strategies[contest.RankingStrategy];
        }

        /// <summary>
        /// Construct a <see cref="RankingSolver"/>.
        /// </summary>
        /// <param name="factory">The contest context factory.</param>
        public RankingSolver(ScopedContestContextFactory factory)
        {
            Store = factory.CreateScoreboard();
            Factory = factory;
        }

        /// <inheritdoc />
        public Task Handle(ScoreboardRefreshEvent notification, CancellationToken cancellationToken)
        {
            return Select(notification.Contest).RefreshCache(Store, notification);
        }

        /// <inheritdoc />
        public async Task Handle(SubmissionCreatedEvent notification, CancellationToken cancellationToken)
        {
            if (notification.Submission.ContestId == 0) return;
            var context = await Factory.CreateAsync(notification.Submission.ContestId);
            var contest = context.Contest;
            if (contest.GetState(notification.Submission.Time) >= ContestState.Ended) return;
            await Select(contest).Pending(Store, contest, notification);
        }

        /// <inheritdoc />
        public async Task Handle(JudgingFinishedEvent notification, CancellationToken cancellationToken)
        {
            if (notification.ContestId == null) return;
            if (!notification.Judging.Active) return;
            var context = await Factory.CreateAsync(notification.ContestId.Value);
            var contest = context.Contest;
            if (contest.GetState(notification.SubmitTime) >= ContestState.Ended) return;

            if (contest.RankingStrategy == 2 && notification.Judging.Status == Verdict.Accepted)
            {
                var problems = await context.FetchProblemsAsync();
                var cfscore = problems.Where(p => p.ProblemId == notification.ProblemId).FirstOrDefault()?.Score ?? 0;
                notification = new JudgingFinishedEvent2(notification, cfscore);
            }

            var strategy = Select(contest);
            await (notification.Judging.Status switch
            {
                Verdict.Accepted => strategy.Accept(Store, contest, notification),
                Verdict.CompileError => strategy.CompileError(Store, contest, notification),
                _ => strategy.Reject(Store, contest, notification)
            });
        }

        /// <inheritdoc />
        public Task<IEnumerable<IScoreboardRow>> Handle(ScoreboardSortEvent request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Select(request.Contest).SortByRule(request.Source, request.IsPublic));
        }
    }
}
