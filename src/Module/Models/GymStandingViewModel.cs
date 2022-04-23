#nullable enable
using System.Linq;
using Xylab.Contesting.Models;

namespace SatelliteSite.ContestModule.Models
{
    public class GymStandingViewModel : FullBoardViewModel
    {
        public ILookup<int, TeamMemberModel> TeamMembers { get; }

        public int Page { get; set; }

        public GymStandingViewModel(
            ScoreboardModel scoreboard,
            ILookup<int, TeamMemberModel> teamMembers)
            : base(scoreboard, true, false)
        {
            TeamMembers = teamMembers;
        }
    }
}
