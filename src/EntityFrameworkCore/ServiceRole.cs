using Ccs.Contexts;
using Ccs.Entities;
using Ccs.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite.IdentityModule.Entities;

namespace Ccs
{
    public class EntityFrameworkCoreServiceRole<TUser, TRole, TContext> : IServiceRole
        where TUser : User, new()
        where TRole : Role, new()
        where TContext : DbContext
    {
        public void Configure(IServiceCollection services)
        {
            services.AddDbModelSupplier<TContext, ContestEntityConfiguration<TUser, TRole, TContext>>();
            services.AddScoped<IContestStore, ContestStore<TContext>>();
            services.AddScoped<IContestRepository>(sp => sp.GetRequiredService<IContestStore>());
            services.AddScoped<IPrintingService, PrintingStore<TUser, TContext>>();
            services.AddScoped<IScoreboard, ScoreboardStore<TContext>>();
            services.AddSingleton<IContestContextFactory, CachedContestContextFactory>();
        }
    }
}
