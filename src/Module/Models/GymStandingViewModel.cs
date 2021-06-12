#nullable enable
using Ccs.Models;
using System.Linq;

namespace SatelliteSite.ContestModule.Models
{
    public class GymStandingViewModel : FullBoardViewModel
    {
        public ILookup<int, string> TeamMembers { get; }

        public GymStandingViewModel(
            ScoreboardModel scoreboard,
            ILookup<int, string> teamMembers)
            : base(scoreboard, true, false)
        {
            TeamMembers = teamMembers;
        }
    }
}
