using Ccs.Entities;
using Ccs.Services;
using MediatR;

namespace Ccs.Events
{
    public class ClarificationCreateEvent : INotification
    {
        public Clarification Clarification { get; }

        public IContestContext Contest { get; }

        public ClarificationCreateEvent(IContestContext contest, Clarification entity)
        {
            Clarification = entity;
            Contest = contest;
        }
    }
}
