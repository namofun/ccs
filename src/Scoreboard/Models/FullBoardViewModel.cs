#nullable disable
using Ccs.Entities;
using Ccs.Scoreboard;
using System;
using System.Collections.Generic;
using System.Linq;
using Tenant.Entities;

namespace Ccs.Models
{
    public class FullBoardViewModel : BoardViewModel
    {
        public DateTimeOffset UpdateTime { get; set; }

        public IEnumerable<IScoreboardRow> RankCache { get; set; }

        public IReadOnlyDictionary<int, Category> Categories { get; set; }

        public IReadOnlyDictionary<int, Affiliation> Affiliations { get; set; }

        public bool IsPublic { get; set; }

        protected override IEnumerable<SortOrderModel> GetEnumerable()
        {
            var rt = RankCache
                .Where(a => Categories.ContainsKey(a.CategoryId))
                .GroupBy(a => Categories[a.CategoryId].SortOrder)
                .OrderBy(g => g.Key);

            foreach (var g in rt)
            {
                var prob = new ProblemStatisticsModel[Problems.Count];
                for (int i = 0; i < Problems.Count; i++)
                    prob[i] = new ProblemStatisticsModel();
                yield return new SortOrderModel(GetViewModel(IsPublic, g, prob), prob);
            }
        }

        private IEnumerable<TeamModel> GetViewModel(
            bool ispublic,
            IEnumerable<IScoreboardRow> src,
            ProblemStatisticsModel[] stat)
        {
            int rank = 0;
            int last_rank = 0;
            int last_point = int.MinValue;
            int last_penalty = int.MinValue;
            var cats = new Dictionary<int, Category>();
            src = RankingSolver.Strategies[RankingStrategy].SortByRule(src, ispublic);

            foreach (var item in src)
            {
                int catid = item.CategoryId;
                string catName = null;
                if (!cats.Keys.Contains(catid))
                {
                    cats.Add(catid, Categories[catid]);
                    catName = Categories[catid].Name;
                }

                int point = ispublic ? item.RankCache.PointsPublic : item.RankCache.PointsRestricted;
                int penalty = ispublic ? item.RankCache.TotalTimePublic : item.RankCache.TotalTimeRestricted;
                rank++;
                if (last_point != point || last_penalty != penalty) last_rank = rank;
                last_point = point;
                last_penalty = penalty;

                var prob = new ScoreCellModel[Problems.Count];

                foreach (var pp in item.ScoreCache)
                {
                    var p = Problems.Find(pp.ProblemId);
                    if (p == null) continue;
                    var pid = p.Rank - 1;

                    if (ispublic)
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

                    if (prob[pid].Score.HasValue)
                    {
                        stat[pid].FirstSolve ??= prob[pid].Score;
                        stat[pid].Accepted++;
                        stat[pid].Rejected += prob[pid].JudgedCount - 1;
                        stat[pid].Pending += prob[pid].PendingCount;
                    }
                    else
                    {
                        stat[pid].Rejected += prob[pid].JudgedCount;
                        stat[pid].Pending += prob[pid].PendingCount;
                    }
                }

                yield return new TeamModel
                {
                    ContestId = IsPublic ? default(int?) : ContestId,
                    TeamId = item.TeamId,
                    TeamName = item.TeamName,
                    Affiliation = Affiliations.GetValueOrDefault(item.AffiliationId)?.Name ?? "",
                    AffiliationId = Affiliations.GetValueOrDefault(item.AffiliationId)?.Abbreviation ?? "null",
                    Category = catName,
                    CategoryColor = cats[catid].Color,
                    Points = point,
                    Penalty = penalty,
                    Rank = last_rank,
                    ShowRank = last_rank == rank,
                    Problems = prob,
                };
            }
        }
    }
}
