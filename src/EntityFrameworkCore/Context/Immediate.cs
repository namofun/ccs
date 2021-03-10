using Ccs.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polygon.Storages;
using System;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class ImmediateContestContext : IContestContext
    {
        private readonly IServiceProvider _services;
        private readonly ContestWrapper _contest;
        private IContestRepository? _ccsFacade;
        private IPolygonFacade? _polygonFacade;

        public IContestInformation Contest => _contest;

        public IPolygonFacade Polygon => _polygonFacade ??= Get<IPolygonFacade>();

        public IContestRepository Ccs => _ccsFacade ??= Get<IContestRepository>();

        public IContestDbContext Db => ((ISupportDbContext)Ccs).Db;

        protected T Get<T>() => _services.GetRequiredService<T>();

        protected ILogger<IContestContext> Logger { get; }

        public ImmediateContestContext(ContestWrapper contest, IServiceProvider serviceProvider)
        {
            _contest = contest;
            _services = serviceProvider;
            Logger = serviceProvider.GetRequiredService<ILogger<IContestContext>>();
        }

        public Task EmitEventAsync(Specifications.AbstractEvent @event, string action)
        {
            Db.ContestEvents.Add(@event.ToEvent(action, Contest.Id));
            return Db.SaveChangesAsync();
        }
    }
}
