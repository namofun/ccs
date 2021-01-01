using Ccs.Entities;
using Ccs.Models;
using System.Collections.Generic;

namespace Ccs
{
    public interface IContestContextAccessor
    {
        IContestContext? Context { get; }

        Contest? Contest { get; }

        IReadOnlyList<ProblemModel>? Problems { get; }

        Team? Team { get; }
    }
}
