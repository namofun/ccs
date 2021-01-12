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
            services.AddScoped<ICcsFacade, CcsFacade<TUser, TContext>>();

            services.AddScoped(sp => sp.GetRequiredService<ICcsFacade>().ContestStore);
            services.AddScoped(sp => sp.GetRequiredService<ICcsFacade>().ProblemStore);
            services.AddScoped<IContestRepository>(sp => sp.GetRequiredService<ICcsFacade>().ContestStore);

            services.AddScoped<IPrintingService, PrintingService<TUser, TContext>>();
            services.AddScoped<IScoreboard, Scoreboard<TContext>>();
            services.AddSingleton<IContestContextFactory, CachedContestContextFactory>();
        }
    }
}
