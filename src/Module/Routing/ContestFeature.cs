#nullable enable
using Ccs;
using Ccs.Entities;

namespace SatelliteSite.ContestModule
{
    public interface IContestFeature
    {
        IContestContext? Context { get; }

        Team? CurrentTeam { get; }
    }

    namespace Routing
    {
        internal class ContestFeature : IContestFeature
        {
            public IContestContext? Context { get; }

            public Team? CurrentTeam { get; set; }

            public ContestFeature(IContestContext? ctx) => Context = ctx;
        }
    }
}
