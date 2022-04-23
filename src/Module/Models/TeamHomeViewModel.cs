#nullable enable
using System.Collections.Generic;
using Xylab.Contesting.Entities;
using Xylab.Contesting.Models;
using Xylab.Tenant.Entities;

namespace SatelliteSite.ContestModule.Models
{
    public class TeamHomeViewModel : BoardViewModel, IScoreboardRow
    {
        public Category Category { get; }

        public Affiliation Affiliation { get; }

        public IReadOnlyList<Clarification> Clarifications { get; }

        public IReadOnlyList<SubmissionViewModel> Submissions { get; }

        public int TeamId { get; }

        public string TeamName { get; }

        public int CategoryId => Category.Id;

        public int AffiliationId => Affiliation.Id;

        public RankCache RankCache { get; }

        public IEnumerable<ScoreCache> ScoreCache { get; }

        public string? TeamLocation { get; }

        public TeamHomeViewModel(
            ScoreboardModel scb,
            int teamid,
            IReadOnlyList<Clarification> clars,
            IReadOnlyList<SubmissionViewModel> submits)
            : base(scb)
        {
            var row = scb.Data[teamid];
            RankCache = row.RankCache;
            ScoreCache = row.ScoreCache;
            TeamId = row.TeamId;
            TeamName = row.TeamName;
            TeamLocation = row.TeamLocation;
            Affiliation = scb.Affiliations[row.AffiliationId];
            Category = scb.Categories[row.CategoryId];
            Clarifications = clars;
            Submissions = submits;
        }

        public override IEnumerator<SortOrderModel> GetEnumerator()
        {
            var team = CreateTeamViewModel(this, Affiliation, Category, false);
            team.ShowRank = true;
            yield return new SortOrderModel(new[] { team }, null);
        }
    }
}
