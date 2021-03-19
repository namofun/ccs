using Ccs.Models;
using Ccs.Services;

namespace Ccs.Events
{
    public class ContestUpdateEvent : IContextedNotification
    {
        public IContestInformation OldContest { get; }

        public IContestInformation NewContest { get; }

        public IContestContext Context { get; }

        public ContestUpdateEvent(IContestInformation old, IContestInformation @new, IContestContext context)
        {
            OldContest = old;
            NewContest = @new;
            Context = context;
        }
    }
}
