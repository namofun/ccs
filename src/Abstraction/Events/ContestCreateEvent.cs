using MediatR;
using Xylab.Contesting.Entities;

namespace Xylab.Contesting.Events
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
