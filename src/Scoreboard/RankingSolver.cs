using Ccs.Entities;
using Ccs.Events;
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
        INotificationHandler<ScoreboardRefreshEvent>,
        IRequestHandler<ScoreboardSortEvent, IEnumerable<Team>>
    {
        /// <summary>
        /// The interface for scoreboard storage
        /// </summary>
        private IScoreboardStore Store { get; }

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
        /// <param name="store">The scoreboard store.</param>
        public RankingSolver(IScoreboardStore store)
        {
            Store = store;
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
            var contest = await Store.FindContestAsync(notification.Submission.ContestId);
            if (contest.GetState(notification.Submission.Time) >= ContestState.Ended) return;
            await Select(contest).Pending(Store, contest, notification);
        }

        /// <inheritdoc />
        public async Task Handle(JudgingFinishedEvent notification, CancellationToken cancellationToken)
        {
            if (notification.ContestId == null) return;
            if (!notification.Judging.Active) return;
            var contest = await Store.FindContestAsync(notification.ContestId.Value);
            if (contest.GetState(notification.SubmitTime) >= ContestState.Ended) return;

            var strategy = Select(contest);
            await (notification.Judging.Status switch
            {
                Verdict.Accepted => strategy.Accept(Store, contest, notification),
                Verdict.CompileError => strategy.CompileError(Store, contest, notification),
                _ => strategy.Reject(Store, contest, notification)
            });
        }

        /// <inheritdoc />
        public Task<IEnumerable<Team>> Handle(ScoreboardSortEvent request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Select(request.Contest).SortByRule(request.Source, request.IsPublic));
        }
    }
}
