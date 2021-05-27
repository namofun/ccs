using Ccs.Entities;
using Ccs.Events;
using Ccs.Models;
using Ccs.Services;
using MediatR;
using Polygon.Entities;
using Polygon.Events;
using System;
using System.Collections.Generic;
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
        INotificationHandler<ScoreboardRefreshEvent>
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
        public static IRankingStrategy Select(IContestInformation contest)
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
        public async Task Handle(ScoreboardRefreshEvent notification, CancellationToken cancellationToken)
        {
            if (notification.Contest.Kind == CcsDefaults.KindProblemset)
            {
                await Store.RebuildStatisticsAsync(notification.Contest.Id);
            }
            else
            {
                var data = await Select(notification.Contest).RefreshCache(Store, notification);
                await Store.RefreshAsync(data);
            }
        }

        /// <inheritdoc />
        public async Task Handle(SubmissionCreatedEvent notification, CancellationToken cancellationToken)
        {
            if (notification.Submission.ContestId == 0) return;
            var context = await Factory.CreateAsync(notification.Submission.ContestId);
            if (context == null || context.Contest.Kind == CcsDefaults.KindProblemset) return;
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
            if (context == null || context.Contest.Kind == CcsDefaults.KindProblemset) return;
            var contest = context.Contest;
            if (contest.GetState(notification.SubmitTime) >= ContestState.Ended) return;

            if (contest.RankingStrategy == CcsDefaults.RuleCodeforces && notification.Judging.Status == Verdict.Accepted)
            {
                var problem = await context.FindProblemAsync(notification.ProblemId);
                var cfscore = problem?.Score ?? 0;
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
    }
}
