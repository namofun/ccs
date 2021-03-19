using Ccs.Services;
using MediatR;

namespace Ccs.Events
{
    public interface IContextedNotification : INotification
    {
        IContestContext Context { get; }
    }
}
