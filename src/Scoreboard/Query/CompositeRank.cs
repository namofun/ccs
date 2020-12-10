using Ccs.Entities;
using Ccs.Events;
using Ccs.Services;
using Polygon.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ccs.Scoreboard.Query
{
    /// <summary>
    /// The composite <see cref="IRankingStrategy"/> for calling.
    /// </summary>
    public class CompositeRank : IRankingStrategy
    {
        /// <summary>
        /// The known strategies
        /// </summary>
        public static readonly IReadOnlyList<IRankingStrategy> Strategies = new IRankingStrategy[]
        {
            new XCPCRank(),
            new OIRank(),
            new CFRank(),
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

        /// <inheritdoc />
        public Task Accept(IScoreboardStore store, Contest contest, JudgingFinishedEvent args)
        {
            return Select(contest).Accept(store, contest, args);
        }

        /// <inheritdoc />
        public Task CompileError(IScoreboardStore store, Contest contest, JudgingFinishedEvent args)
        {
            return Select(contest).CompileError(store, contest, args);
        }

        /// <inheritdoc />
        public Task Pending(IScoreboardStore store, Contest contest, SubmissionCreatedEvent args)
        {
            return Select(contest).Pending(store, contest, args);
        }

        /// <inheritdoc />
        public Task RefreshCache(IScoreboardStore store, ScoreboardRefreshEvent args)
        {
            return Select(args.Contest).RefreshCache(store, args);
        }

        /// <inheritdoc />
        public Task Reject(IScoreboardStore store, Contest contest, JudgingFinishedEvent args)
        {
            return Select(contest).Reject(store, contest, args);
        }

        /// <inheritdoc />
        public IEnumerable<Team> SortByRule(IEnumerable<Team> source, Contest contest, bool isPublic)
        {
            return Select(contest).SortByRule(source, contest, isPublic);
        }
    }
}
