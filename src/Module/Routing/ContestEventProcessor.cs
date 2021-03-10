using Ccs.Entities;
using Ccs.Events;
using Ccs.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public class ContestEventProcessor :
        INotificationHandler<ClarificationCreateEvent>
    {
        public IScoreboard Scoreboard { get; }

        public ContestEventProcessor(IScoreboard scoreboard)
        {
            Scoreboard = scoreboard;
        }

        public Task Handle(ClarificationCreateEvent notification, CancellationToken cancellationToken)
        {
            if (notification.Contest.GetState() == ContestState.NotScheduled)
            {
                return Task.CompletedTask;
            }

            var spec = new Specifications.Clarification(notification.Clarification, notification.Contest.StartTime!.Value);
            return Scoreboard.EmitEventAsync(spec.ToEvent("create", notification.Contest.Id));
        }
    }
}
