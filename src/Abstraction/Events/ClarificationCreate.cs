using Ccs.Entities;
using Ccs.Services;
using MediatR;

namespace Ccs.Events
{
    public class ClarificationCreateEvent : INotification
    {
        public Clarification Clarification { get; }

        public IContestContextAccessor Contest { get; }

        public ClarificationCreateEvent(IContestContextAccessor contest, Clarification entity)
        {
            Clarification = entity;
            Contest = contest;
        }
    }
}
