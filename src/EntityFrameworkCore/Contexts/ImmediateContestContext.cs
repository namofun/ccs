using Ccs.Entities;
using Microsoft.EntityFrameworkCore;
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

        public IContestStore ContestStore => Ccs.ContestStore;

        public ImmediateContestContext(Contest contest, IServiceProvider serviceProvider)
        {
            _contest = contest;
            _services = serviceProvider;
        }

        protected T Get<T>() => _services.GetRequiredService<T>();

        public virtual async Task<object> GetUpdatesAsync()
        {
            int cid = Contest.Id;
            var clarifications = Ccs.Clarifications.CountAsync(c => c.ContestId == cid && !c.Answered);
            var rejudgings = await Polygon.Rejudgings.CountUndoneAsync(Contest.Id);
            var teams = await Ccs.Teams.CountAsync(t => t.ContestId == cid && t.Status == 0);
            return new { clarifications, teams, rejudgings };
        }
    }
}
