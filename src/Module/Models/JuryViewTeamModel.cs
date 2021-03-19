#nullable disable
using Ccs.Entities;
using Ccs.Models;
using Polygon.Models;
using System.Collections.Generic;
using Tenant.Entities;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryViewTeamModel : BoardViewModel, IScoreboardRow
    {
        public List<Solution> Solutions { get; set; }

        public Category Category { get; set; }

        public Affiliation Affiliation { get; set; }

        public IEnumerable<string> Members { get; set; }

        public int TeamId { get; set; }

        public string TeamName { get; set; }

        public int Kind { get; set; }

        public int Status { get; set; }

        public int CategoryId => Category.Id;

        public int AffiliationId => Affiliation.Id;

        public RankCache RankCache { get; set; }

        public IEnumerable<ScoreCache> ScoreCache { get; set; }

        protected override IEnumerable<SortOrderModel> GetEnumerable()
        {
            yield return new SortOrderModel(GetSingleScore(), null);
        }

        private IEnumerable<TeamModel> GetSingleScore()
        {
            var prob = new ScoreCellModel[Problems.Count];

            foreach (var pp in ScoreCache)
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
                TeamId = TeamId,
                TeamName = TeamName,
                Affiliation = Affiliation.Name,
                AffiliationId = Affiliation.Abbreviation,
                Category = Category.Name,
                CategoryColor = Category.Color,
                Points = RankCache.PointsRestricted,
                Penalty = RankCache.TotalTimeRestricted,
                ShowRank = true,
                Problems = prob,
            };
        }
    }
}
