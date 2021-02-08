using Ccs.Models;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Storages;
using System;

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

        public ImmediateContestContext(ContestWrapper contest, IServiceProvider serviceProvider)
        {
            _contest = contest;
            _services = serviceProvider;
        }
    }
}
