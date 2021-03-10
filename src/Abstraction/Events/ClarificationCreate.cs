using Ccs.Entities;
using Ccs.Models;
using MediatR;

namespace Ccs.Events
{
    public class ClarificationCreateEvent : INotification
    {
        public Clarification Clarification { get; }

        public IContestInformation Contest { get; }

        public ClarificationCreateEvent(IContestInformation contest, Clarification entity)
        {
            Clarification = entity;
            Contest = contest;
        }
    }
}
