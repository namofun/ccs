using Ccs.Entities;
using Ccs.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite.IdentityModule.Entities;

namespace Ccs
{
    public class RelationalRole<TUser, TRole, TContext> : IServiceRole
        where TUser : User, new()
        where TRole : Role, new()
        where TContext : DbContext, IContestDbContext
    {
        public void Configure(IServiceCollection services)
        {
            services.AddDbModelSupplier<TContext, ContestEntityConfiguration<TUser, TRole, TContext>>();

            services.AddScoped<IContestRepository, ContestRepository<TContext>>();
            services.AddScoped<IPrintingService, PrintingService<TContext>>();
            services.AddScoped<IScoreboard, Scoreboard<TContext>>();

            services.AddSingleton<IContestContextFactory, CachedContestContextFactory>();
        }
    }
}
