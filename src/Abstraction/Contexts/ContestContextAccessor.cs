using Ccs.Entities;
using Ccs.Models;
using System.Collections.Generic;

namespace Ccs
{
    public interface IContestContextAccessor
    {
        IContestContext? Context { get; }

        Contest? Contest { get; }

        ProblemCollection? Problems { get; }

        Team? Team { get; }
    }
}
