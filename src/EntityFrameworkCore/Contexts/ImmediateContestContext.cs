using Ccs.Entities;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Storages;
using System;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class ImmediateContestContext : IContestContext
    {
        private readonly IServiceProvider _services;
        private readonly Contest _contest;
        private ICcsFacade? _ccsFacade;
        private IPolygonFacade? _polygonFacade;

        public Contest Contest => _contest;

        public IPolygonFacade Polygon => _polygonFacade ??= _services.GetRequiredService<IPolygonFacade>();

        public ICcsFacade Ccs => _ccsFacade ??= _services.GetRequiredService<ICcsFacade>();

        public IClarificationStore ClarificationStore => Ccs.ClarificationStore;

        public ITeamStore TeamStore => Ccs.TeamStore;

        public IContestStore ContestStore => Ccs.ContestStore;

        public IRejudgingStore RejudgingStore => Polygon.Rejudgings;

        public ImmediateContestContext(Contest contest, IServiceProvider serviceProvider)
        {
            _contest = contest;
            _services = serviceProvider;
        }

        public virtual async Task<object> GetUpdatesAsync()
        {
            return new
            {
                clarifications = await ClarificationStore.CountUnansweredAsync(Contest.Id),
                teams = await TeamStore.CountPendingAsync(Contest.Id),
                rejudgings = await RejudgingStore.CountUndoneAsync(Contest.Id),
            };
        }
    }
}
