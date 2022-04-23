using Xylab.Contesting.Models;
using Xylab.Contesting.Services;

namespace Xylab.Contesting.Events
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
