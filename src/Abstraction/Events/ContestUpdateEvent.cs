using Ccs.Entities;
using MediatR;

namespace Ccs.Events
{
    public class ContestUpdateEvent : INotification
    {
        public Contest OldContest { get; }

        public Contest NewContest { get; }

        public ContestUpdateEvent(Contest old, Contest @new)
        {
            OldContest = old;
            NewContest = @new;
        }
    }
}
