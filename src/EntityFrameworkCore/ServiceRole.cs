using Ccs.Entities;
using Ccs.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Storages;
using SatelliteSite.IdentityModule.Entities;

namespace Ccs
{
    public class RelationalRole<TUser, TRole, TContext> : IServiceRole
        where TUser : User, new()
        where TRole : Role, new()
        where TContext : DbContext, IContestDbContext, IPolygonDbContext
    {
        public void Configure(IServiceCollection services)
        {
            var userType = typeof(TUser);
            var ratingUpdaterType = typeof(NullRatingUpdater);
            if (typeof(IUserWithRating).IsAssignableFrom(userType))
            {
                ratingUpdaterType = typeof(RatingUpdater<,>).MakeGenericType(userType, typeof(TContext));
            }

            services.AddDbModelSupplier<TContext, ContestEntityConfiguration<TUser, TRole, TContext>>();

            services.AddScoped<IContestRepository, ContestRepository<TContext>>();
            services.AddScoped<IPrintingService, PrintingService<TContext>>();
            services.AddScoped<IScoreboard, Scoreboard<TContext>>();
            services.Add(ServiceDescriptor.Scoped(typeof(IRatingUpdater), ratingUpdaterType));

            services.AddSingleton<IContestContextFactory, CachedContestContextFactory>();
        }
    }
}
