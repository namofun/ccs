using Ccs.Entities;
using Ccs.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Options;
using Polygon.Storages;
using SatelliteSite.IdentityModule.Entities;
using System.IO;

namespace Ccs
{
    public class RelationalRole<TUser, TRole, TContext> : IServiceRole
        where TUser : User, new()
        where TRole : Role, new()
        where TContext : DbContext, IContestDbContext, IPolygonDbContext
    {
        private static void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public void Configure(IServiceCollection services)
        {
            if (typeof(IUserWithRating).IsAssignableFrom(typeof(TUser)))
            {
                CcsDefaults.SupportsRating = true;
                services.AddScoped(typeof(IRatingUpdater), typeof(RatingUpdater<,>).MakeGenericType(typeof(TUser), typeof(TContext)));
                services.AddScoped<Microsoft.AspNetCore.Identity.IUserClaimsProvider, RatingClaimsProvider>();
            }
            else
            {
                services.AddDbModelSupplier<TContext, RemoveRatingRelatedConfiguration<TContext>>();
                services.AddScoped<IRatingUpdater, NullRatingUpdater>();
            }

            services.AddDbModelSupplier<TContext, ContestEntityConfiguration<TUser, TRole, TContext>>();

            services.AddSingleton<CachedContestRepository2Cache>();
            services.AddMediatRAssembly(typeof(CachedContestRepository2CacheCleaner).Assembly);
            services.AddScoped<IContestRepository, ContestRepository<TContext>>();
            services.AddScoped<IPrintingService, PrintingService<TContext>>();
            services.AddScoped<IScoreboard, Scoreboard<TContext>>();
            services.AddScoped<IContestRepository2, CachedContestRepository2<TContext>>();

            services.AddSingleton<IContestContextFactory, CachedContestContextFactory>();

            services.AddOptions<ContestFileOptions>()
            .PostConfigure<IOptions<Polygon.PolygonPhysicalOptions>>((options, polygon) =>
            {
                if (options.ContestFileProvider == null)
                {
                    if (string.IsNullOrEmpty(options.ContestDirectory)
                        && !string.IsNullOrEmpty(polygon.Value.ProblemDirectory))
                    {
                        options.ContestDirectory = polygon.Value.ProblemDirectory;
                    }

                    EnsureDirectoryExists(options.ContestDirectory);
                }
            });

            services.AddSingleton<IContestFileProvider>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<ContestFileOptions>>();
                return options.Value.ContestFileProvider ?? new ContestFileProvider(new PhysicalBlobProvider(options.Value.ContestDirectory));
            });
        }
    }
}
