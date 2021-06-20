#nullable enable
using Ccs.Entities;
using Ccs.Models;
using Polygon.Models;
using System.Collections.Generic;
using Tenant.Entities;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryViewTeamModel
    {
        public IReadOnlyList<Solution> Solutions { get; }

        public Category Category { get; }

        public Affiliation Affiliation { get; }

        public IReadOnlyList<TeamMemberModel> Members { get; }

        public int Status { get; }

        public int TeamId { get; }

        public string TeamName { get; }

        public string? TeamLocation { get; }

        public BoardViewModel? Board { get; }

        public JuryViewTeamModel(
            Team team,
            Category category,
            Affiliation affiliation,
            IReadOnlyList<TeamMemberModel> members,
            IReadOnlyList<Solution> solutions,
            ScoreboardModel scoreboard)
        {
            Solutions = solutions;
            Category = category;
            Affiliation = affiliation;
            Members = members;
            Status = team.Status;
            TeamId = team.TeamId;
            TeamName = team.TeamName;
            TeamLocation = team.Location;

            if (scoreboard.Data.ContainsKey(TeamId))
            {
                Board = new TinyBoard(scoreboard, TeamId, category, affiliation);
            }
        }

        private class TinyBoard : BoardViewModel
        {
            private readonly IScoreboardRow _row;
            private readonly Category _category;
            private readonly Affiliation _affiliation;

            public TinyBoard(
                ScoreboardModel scb,
                int teamid,
                Category category,
                Affiliation affiliation)
                : base(scb)
            {
                _row = scb.Data[teamid];
                _category = category;
                _affiliation = affiliation;
            }

            public override IEnumerator<SortOrderModel> GetEnumerator()
            {
                var team = CreateTeamViewModel(_row, _affiliation, _category, false);
                team.ShowRank = true;
                yield return new SortOrderModel(new[] { team }, null);
            }
        }
    }
}
