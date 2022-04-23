using Xylab.Contesting.Entities;
using Xylab.Contesting.Services;

namespace Xylab.Contesting.Events
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
