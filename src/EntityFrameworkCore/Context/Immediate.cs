using Ccs.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polygon.Storages;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class ImmediateContestContext : IContestContext
    {
        private readonly IServiceProvider _services;
        private readonly ContestWrapper _contest;
        private IContestRepository? _ccsFacade;
        private IPolygonFacade? _polygonFacade;
        private IMediator? _mediator;

        public IContestInformation Contest => _contest;

        public IPolygonFacade Polygon => _polygonFacade ??= Get<IPolygonFacade>();

        public IContestRepository Ccs => _ccsFacade ??= Get<IContestRepository>();

        public IContestDbContext Db => ((ISupportDbContext)Ccs).Db;

        public IMediator Mediator => _mediator ??= Get<IMediator>();

        protected T Get<T>() where T : class => _services.GetRequiredService<T>();

        protected ILogger<IContestContext> Logger { get; }

        public ImmediateContestContext(ContestWrapper contest, IServiceProvider serviceProvider)
        {
            _contest = contest;
            _services = serviceProvider;
            Logger = serviceProvider.GetRequiredService<ILogger<IContestContext>>();
        }

        protected virtual async Task<Specifications.State> GetLastStateAsync()
        {
            if (Contest.Settings.EventAvailable)
            {
                var @event = await Db.ContestEvents
                    .Where(e => e.EndpointType == "state")
                    .OrderByDescending(e => e.Id)
                    .FirstOrDefaultAsync();

                if (@event != null)
                {
                    try
                    {
                        return @event.Content.AsJson<Specifications.State>();
                    }
                    catch (System.Text.Json.JsonException)
                    {
                    }
                }
            }

            return new Specifications.State();
        }

        protected virtual async Task WriteLastStateAsync(
            Entities.ContestState now,
            Entities.ContestState lastState)
        {
            if (lastState < Entities.ContestState.Started
                && now < Entities.ContestState.Started)
                return;

            var state = new Specifications.State(Contest, now switch
            {
                Entities.ContestState.Finalized => Contest.StartTime + (Contest.UnfreezeTime ?? Contest.EndTime),
                Entities.ContestState.Ended => Contest.StartTime + Contest.EndTime,
                Entities.ContestState.Frozen => Contest.StartTime + Contest.FreezeTime,
                Entities.ContestState.Started => Contest.StartTime,
                _ => Contest.StartTime?.AddDays(-1),
            });

            Db.ContestEvents.Add(state.ToEvent("update", Contest.Id));
            await Db.SaveChangesAsync();
        }

        public async Task EnsureLastStateAsync()
        {
            if (!Contest.Settings.EventAvailable) return;
            var state = Contest.GetState();
            var lastState = (await GetLastStateAsync()).GetState();
            if (state != lastState) await WriteLastStateAsync(state, lastState);
        }

        public async Task EmitEventAsync(Specifications.AbstractEvent @event, string action)
        {
            if (!Contest.Settings.EventAvailable) return;
            await EnsureLastStateAsync();
            Db.ContestEvents.Add(@event.ToEvent(action, Contest.Id));
            await Db.SaveChangesAsync();
        }

        public async Task EmitEventAsync(EventBatch events)
        {
            if (!Contest.Settings.EventAvailable) return;

            await events.LogAsync($"A batch of {events.Count} events has been submitted.");
            int last = 0, current = 0;

            foreach (var g in events.GroupBy(e => e.EventTime).OrderBy(gg => gg.Key))
            {
                foreach (var e in g)
                {
                    Db.ContestEvents.Add(e);
                    current += await Db.SaveChangesAsync();

                    if (current / 50 != last)
                    {
                        last = current / 50;
                        await events.LogAsync($"Processing... ({current} / {events.Count})");
                    }
                }
            }

            await events.LogAsync("Finished.\n");
        }
    }
}
