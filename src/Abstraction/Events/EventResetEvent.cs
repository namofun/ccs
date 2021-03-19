using Ccs.Models;
using Ccs.Services;

namespace Ccs.Events
{
    public class EventResetEvent : IContextedNotification
    {
        public IContestContext Context { get; }

        public IContestInformation Contest => Context.Contest;

        public EventResetEvent(IContestContext context)
        {
            Context = context;
        }
    }
}
