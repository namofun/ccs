using Ccs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Storages;
using SatelliteSite.Entities;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class ImmediateContestContext : IContestContext, ISupportDbContext
    {
        private readonly IServiceProvider _services;
        private readonly Contest _contest;
        private IContestRepository? _ccsFacade;
        private IPolygonFacade? _polygonFacade;

        public Contest Contest => _contest;

        public IPolygonFacade Polygon => _polygonFacade ??= Get<IPolygonFacade>();

        public IContestRepository Ccs => _ccsFacade ??= Get<IContestRepository>();

        public IContestDbContext Db => ((ISupportDbContext)Ccs).Db;

        public ImmediateContestContext(Contest contest, IServiceProvider serviceProvider)
        {
            _contest = contest;
            _services = serviceProvider;
        }

        protected T Get<T>() => _services.GetRequiredService<T>();

        public Task<IPagedList<Auditlog>> ViewLogsAsync(int page, int pageCount)
        {
            return Get<IAuditlogger>().ViewLogsAsync(Contest.Id, page, pageCount);
        }

        public virtual async Task<object> GetUpdatesAsync()
        {
            int cid = Contest.Id;
            var clarifications = Db.Clarifications.CountAsync(c => c.ContestId == cid && !c.Answered);
            var rejudgings = await Polygon.Rejudgings.CountUndoneAsync(Contest.Id);
            var teams = await Db.Teams.CountAsync(t => t.ContestId == cid && t.Status == 0);
            return new { clarifications, teams, rejudgings };
        }
    }
}
