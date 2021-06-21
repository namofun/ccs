using Ccs.Entities;
using MediatR;

namespace Ccs.Events
{
    public class ContestCreateEvent : INotification
    {
        public Contest Contest { get; }

        public bool PostCreation { get; }

        public ContestCreateEvent(Contest contest, bool postCreate)
        {
            Contest = contest;
            PostCreation = postCreate;
        }
    }
}
