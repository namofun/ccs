using Ccs.Entities;
using Ccs.Services;

namespace Ccs.Events
{
    public class ClarificationCreateEvent : IContextedNotification
    {
        public Clarification Clarification { get; }

        public IContestContext Context { get; }

        public ClarificationCreateEvent(IContestContext contest, Clarification entity)
        {
            Clarification = entity;
            Context = contest;
        }
    }
}
