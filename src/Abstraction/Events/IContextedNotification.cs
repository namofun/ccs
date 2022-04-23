using MediatR;
using Xylab.Contesting.Services;

namespace Xylab.Contesting.Events
{
    public interface IContextedNotification : INotification
    {
        IContestContext Context { get; }
    }
}
