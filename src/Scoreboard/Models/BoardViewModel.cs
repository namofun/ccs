#nullable enable
using System.Collections;
using System.Collections.Generic;

namespace Ccs.Models
{
    public abstract class BoardViewModel : IEnumerable<SortOrderModel>
    {
        public int ContestId { get; }

        public int RankingStrategy { get; }

        public ProblemCollection Problems { get; }

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

                if (RankingStrategy == CcsDefaults.RuleCodeforces)
                {
                    prob[pid] = new ScoreCellModel
                    {
                        PendingCount = pp.PendingRestricted,
                        JudgedCount = pp.SubmissionPublic,
                        Score = pp.ScorePublic,
                        SolveTime = pp.SolveTimePublic,
                        FailedSystemTest = pp.FirstToSolve,
                    };
                }
                else if (ispublic)
                {
                    prob[pid] = new ScoreCellModel
                    {
                        PendingCount = pp.PendingPublic,
                        IsFirstToSolve = pp.FirstToSolve,
                        JudgedCount = pp.SubmissionPublic,
                        Score = pp.ScorePublic,
                        SolveTime = pp.SolveTimePublic,
                    };
                }
                else
                {
                    prob[pid] = new ScoreCellModel
                    {
                        PendingCount = pp.PendingRestricted,
                        IsFirstToSolve = pp.FirstToSolve,
                        JudgedCount = pp.SubmissionRestricted,
                        Score = pp.ScoreRestricted,
                        SolveTime = pp.SolveTimeRestricted,
                    };
                }

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

            ispublic |= RankingStrategy == CcsDefaults.RuleCodeforces;
            return new TeamModel
            {
                TeamId = row.TeamId,
                TeamName = row.TeamName,
                Affiliation = aff?.Name ?? "",
                AffiliationId = aff?.Abbreviation ?? "null",
                Category = cat?.Name ?? "",
                CategoryColor = cat?.Color ?? "#ffffff",
                Points = ispublic ? row.RankCache.PointsPublic : row.RankCache.PointsRestricted,
                Penalty = ispublic ? row.RankCache.TotalTimePublic : row.RankCache.TotalTimeRestricted,
                LastAc = ispublic ? row.RankCache.LastAcPublic : row.RankCache.LastAcRestricted,
                Problems = prob,
            };
        }

        protected BoardViewModel(int cid, int rule, ProblemCollection problems)
        {
            ContestId = cid;
            RankingStrategy = rule;
            Problems = problems;
        }
    }
}
