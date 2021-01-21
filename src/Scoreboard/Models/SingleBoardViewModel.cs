#nullable disable
using Ccs.Entities;
using System.Collections.Generic;
using System.Linq;
using Tenant.Entities;

namespace Ccs.Models
{
    public class SingleBoardViewModel : BoardViewModel
    {
        public IScoreboardRow QueryInfo { get; set; }

        public Category Category { get; set; }

        public Affiliation Affiliation { get; set; }

        protected override IEnumerable<SortOrderModel> GetEnumerable()
        {
            yield return new SortOrderModel(GetSingleScore(), null);
        }

        private IEnumerable<TeamModel> GetSingleScore()
        {
            var prob = new ScoreCellModel[Problems.Count];

            foreach (var pp in QueryInfo.ScoreCache ?? Enumerable.Empty<ScoreCache>())
            {
                var p = Problems.Find(pp.ProblemId);
                if (p == null) continue;
                var pid = p.Rank - 1;

                prob[pid] = new ScoreCellModel
                {
                    PendingCount = pp.PendingRestricted,
                    IsFirstToSolve = pp.FirstToSolve,
                    JudgedCount = pp.SubmissionRestricted,
                    Score = pp.ScoreRestricted,
                    SolveTime = pp.SolveTimeRestricted,
                };
            }

            yield return new TeamModel
            {
                TeamId = QueryInfo.TeamId,
                TeamName = QueryInfo.TeamName,
                Affiliation = Affiliation.Name,
                AffiliationId = Affiliation.Abbreviation,
                Category = Category.Name,
                CategoryColor = Category.Color,
                Points = QueryInfo.RankCache?.PointsRestricted ?? 0,
                Penalty = QueryInfo.RankCache?.TotalTimeRestricted ?? 0,
                ShowRank = true,
                Problems = prob,
            };
        }
    }
}
