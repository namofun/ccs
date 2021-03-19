#nullable disable
using Ccs.Entities;
using Ccs.Models;
using Ccs.Scoreboard;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SatelliteSite.ContestModule.Models
{
    public class GymStandingViewModel : BoardViewModel
    {
        public DateTimeOffset UpdateTime { get; set; }

        public IEnumerable<IScoreboardRow> RankCache { get; set; }

        public HashSet<int> OrganizationIds { get; set; }

        public ILookup<int, string> TeamMembers { get; set; }

        public int CurrentPage { get; set; }

        public IReadOnlyDictionary<int, (int Accepted, int Total, int AccTeam, int TotTeam)> Statistics { get; set; }

        public SortOrderModel CreateDefault()
        {
            var stat = new ProblemStatisticsModel[Problems.Count];
            for (int i = 0; i < Problems.Count; i++)
            {
                var (ac, tot, acc, tol) = Statistics.GetValueOrDefault(Problems[i].ProblemId);
                stat[i] = new ProblemStatisticsModel { Accepted = acc, Rejected = tot - ac };
            }

            return new SortOrderModel(GetViewModel(RankCache), stat);
        }

        protected override IEnumerable<SortOrderModel> GetEnumerable()
        {
            yield return CreateDefault();
        }

        private IEnumerable<TeamModel> GetViewModel(IEnumerable<IScoreboardRow> src)
        {
            int rank = 0;
            int last_rank = 0;
            int last_point = int.MinValue;
            int last_penalty = int.MinValue;
            src = RankingSolver.Strategies[RankingStrategy].SortByRule(src, true);

            foreach (var item in src)
            {
                int point = item.RankCache.PointsPublic;
                int penalty = item.RankCache.TotalTimePublic;
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

                    prob[pid] = new ScoreCellModel
                    {
                        PendingCount = pp.PendingPublic,
                        IsFirstToSolve = pp.FirstToSolve,
                        JudgedCount = pp.SubmissionPublic,
                        Score = pp.ScorePublic,
                        SolveTime = pp.SolveTimePublic,
                    };
                }

                yield return new TeamModel
                {
                    TeamId = item.TeamId,
                    TeamName = item.TeamName,
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
