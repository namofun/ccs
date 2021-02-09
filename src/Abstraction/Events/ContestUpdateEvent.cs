using Ccs.Models;
using MediatR;

namespace Ccs.Events
{
    public class ContestUpdateEvent : INotification
    {
        public IContestInformation OldContest { get; }

        public IContestInformation NewContest { get; }

        public ContestUpdateEvent(IContestInformation old, IContestInformation @new)
        {
            OldContest = old;
            NewContest = @new;
        }
    }
}
