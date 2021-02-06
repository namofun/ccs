#nullable disable
using Ccs.Entities;
using Ccs.Models;
using Polygon.Models;
using System.Collections.Generic;
using System.Linq;
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

        public ICollection<ScoreCache> ScoreCache { get; set; }

        protected override IEnumerable<SortOrderModel> GetEnumerable()
        {
            yield return new SortOrderModel(GetSingleScore(), null);
        }

        private IEnumerable<TeamModel> GetSingleScore()
        {
            var prob = new ScoreCellModel[Problems.Count];

            foreach (var pp in ScoreCache ?? Enumerable.Empty<ScoreCache>())
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
                Points = RankCache?.PointsRestricted ?? 0,
                Penalty = RankCache?.TotalTimeRestricted ?? 0,
                ShowRank = true,
                Problems = prob,
            };
        }
    }
}
