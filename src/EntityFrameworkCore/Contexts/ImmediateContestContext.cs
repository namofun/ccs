using Ccs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Storages;
using SatelliteSite.Entities;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class ImmediateContestContext : IContestContext, ISupportDbContext, IContestQueryableStore
    {
        private readonly IServiceProvider _services;
        private readonly Contest _contest;
        private IContestRepository? _ccsFacade;
        private IPolygonFacade? _polygonFacade;

        public IContestInformation Contest => _contest;

        public IPolygonFacade Polygon => _polygonFacade ??= Get<IPolygonFacade>();

        public IContestRepository Ccs => _ccsFacade ??= Get<IContestRepository>();

        public IContestDbContext Db => ((ISupportDbContext)Ccs).Db;

        IQueryable<Contest> IContestQueryableStore.Contests => Db.Contests;
        IQueryable<ContestProblem> IContestQueryableStore.ContestProblems => Db.ContestProblems;
        IQueryable<Jury> IContestQueryableStore.ContestJuries => Db.ContestJuries;
        IQueryable<Team> IContestQueryableStore.Teams => Db.Teams;
        IQueryable<Member> IContestQueryableStore.TeamMembers => Db.TeamMembers;
        IQueryable<Clarification> IContestQueryableStore.Clarifications => Db.Clarifications;
        IQueryable<Balloon> IContestQueryableStore.Balloons => Db.Balloons;
        IQueryable<Event> IContestQueryableStore.ContestEvents => Db.ContestEvents;
        IQueryable<Printing> IContestQueryableStore.Printings => Db.Printings;
        IQueryable<RankCache> IContestQueryableStore.RankCache => Db.RankCache;
        IQueryable<ScoreCache> IContestQueryableStore.ScoreCache => Db.ScoreCache;

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
            var clarifications = await Db.Clarifications.CountAsync(c => c.ContestId == cid && !c.Answered);
            var rejudgings = await Polygon.Rejudgings.CountUndoneAsync(Contest.Id);
            var teams = await Db.Teams.CountAsync(t => t.ContestId == cid && t.Status == 0);
            return new { clarifications, teams, rejudgings };
        }
    }
}
