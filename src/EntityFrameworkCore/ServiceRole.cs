﻿using Ccs.Entities;
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
            if (typeof(IUserWithRating).IsAssignableFrom(typeof(TUser)))
            {
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
        }
    }
}
