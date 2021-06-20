#nullable enable
using Ccs.Scoreboard;
using System.Collections;
using System.Collections.Generic;

namespace Ccs.Models
{
    public abstract class BoardViewModel : IEnumerable<SortOrderModel>
    {
        public int ContestId { get; }

        public IRankingStrategyV2 RankingStrategy { get; }

        public ProblemCollection Problems { get; }

        public IContestTime ContestTime { get; }

        public abstract IEnumerator<SortOrderModel> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected TeamModel CreateTeamViewModel(
            IScoreboardRow row,
            Tenant.Entities.Affiliation? aff,
            Tenant.Entities.Category? cat,
            bool ispublic,
            ProblemStatisticsModel[]? stat = null)
        {
            var prob = new ScoreCellModel[Problems.Count];

            foreach (var pp in row.ScoreCache)
            {
                var p = Problems.Find(pp.ProblemId);
                if (p == null) continue;
                var pid = p.Rank - 1;
                prob[pid] = RankingStrategy.ToCell(pp, ispublic);

                if (stat == null) continue;
                if (prob[pid].Score.HasValue)
                {
                    var score = prob[pid].Score!.Value;
                    if (prob[pid].IsFirstToSolve) stat[pid].FirstSolve ??= score;
                    stat[pid].Accepted++;
                    stat[pid].Rejected += prob[pid].JudgedCount - 1;
                    stat[pid].Pending += prob[pid].PendingCount;
                    stat[pid].MaxScore = System.Math.Max(stat[pid].MaxScore, score);
                }
                else
                {
                    stat[pid].Rejected += prob[pid].JudgedCount;
                    stat[pid].Pending += prob[pid].PendingCount;
                }
            }

            var (points, penalty, lastac) = RankingStrategy.GetRanks(row.RankCache, ispublic);
            return new TeamModel
            {
                TeamId = row.TeamId,
                TeamName = row.TeamName,
                Affiliation = aff?.Name ?? "",
                AffiliationId = aff?.Abbreviation ?? "null",
                Category = cat?.Name ?? "",
                CategoryColor = cat?.Color ?? "#ffffff",
                Points = points,
                Penalty = penalty,
                LastAc = lastac,
                Problems = prob,
            };
        }

        protected BoardViewModel(int cid, IRankingStrategy rule, ProblemCollection problems, IContestTime time)
        {
            ContestId = cid;
            RankingStrategy = (rule as IRankingStrategyV2) ?? NullRank.Instance;
            Problems = problems;
            ContestTime = time;
        }

        protected BoardViewModel(ScoreboardModel scb)
            : this(scb.ContestId, scb.RankingStrategy, scb.Problems, scb.ContestTime)
        {
        }
    }
}
