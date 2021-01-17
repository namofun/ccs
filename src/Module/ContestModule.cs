using Ccs;
using Ccs.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite.ContestModule.Routing;
using System;
using System.Runtime.CompilerServices;

namespace SatelliteSite.ContestModule
{
    public class ContestModule<TRole> : AbstractModule
        where TRole : class, IServiceRole, new()
    {
        public override string Area => "Contest";

        public override void Initialize()
        {
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapControllers();

            endpoints.MapApiDocument(
                name: "ccsapi",
                title: "Contest Module",
                description: "ICPC Contest API (compatible as CCS)",
                version: "2020");
        }

        private static void EnsureRegistered<TService>(IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var serviceType = typeof(TService);

            for (int i = 0; i < services.Count; i++)
            {
                if (services[i].ServiceType != serviceType) continue;
                if (services[i].Lifetime != serviceLifetime)
                    throw new InvalidOperationException(
                        $"The service lifetime for {serviceType} is not correct.");
                return;
            }

            throw new InvalidOperationException(
                $"No implementation for {serviceType} was registered.");
        }

        public override void RegisterServices(IServiceCollection services)
        {
            new TRole().Configure(services);

            services.AddScoped<ScopedContestContextFactory>();
            EnsureRegistered<IContestContextFactory>(services, ServiceLifetime.Singleton);
            EnsureRegistered<IScoreboard>(services);
            EnsureRegistered<IPrintingService>(services);
            EnsureRegistered<IContestRepository>(services);

            services.AddScoped<ContestFeature>();
            services.AddScoped<IContestContextAccessor>(sp => sp.GetRequiredService<ContestFeature>());
            services.AddScoped<IContestFeature>(sp => sp.GetRequiredService<ContestFeature>());

            services.AddMediatRAssembly(typeof(Ccs.Scoreboard.RankingSolver).Assembly);

            services.ConfigureApplicationBuilder(options =>
            {
                options.PointBeforeRouting.Add(app => app.UseMiddleware<InitializeContestMiddleware>());
                options.PointBetweenAuth.Add(app => app.UseMiddleware<InitializeTeamMiddleware>());
            });

            services.ConfigureRouting(options =>
            {
                options.ConstraintMap.Add("c", typeof(ContestRouteConstraint));
            });
        }

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Menu(MenuNameDefaults.DashboardNavbar, menu =>
            {
                menu.HasEntry(400)
                    .HasTitle("fas fa-trophy", "Contests")
                    .HasLink("Dashboard", "Contests", "List")
                    .ActiveWhenController("Contests");
            });

            menus.Submenu(MenuNameDefaults.DashboardUsers, menu =>
            {
                menu.HasEntry(150)
                    .HasTitle(string.Empty, "Contests")
                    .HasLink("Dashboard", "Contests", "List");
            });

            menus.Submenu(MenuNameDefaults.DashboardDocuments, menu =>
            {
                menu.HasEntry(151)
                    .HasTitle(string.Empty, "ICPC Contest API")
                    .HasLink("/api/doc/ccsapi");
            });

            menus.Menu(CcsDefaults.JuryNavbar, menu =>
            {
                menu.HasEntry(100)
                    .HasLink("Contest", "Teams", "List")
                    .HasTitle("fas fa-address-book", "Teams")
                    .HasIdentifier("menu_teams")
                    .HasBadge("teams", BootstrapColor.warning)
                    .ActiveWhenController("Teams");

                menu.HasEntry(200)
                    .HasLink("Contest", "Clarifications", "List")
                    .HasTitle("fas fa-comments", "Clarifications")
                    .HasIdentifier("menu_clarifications")
                    .HasBadge("clarifications", BootstrapColor.info)
                    .ActiveWhenController("Clarifications");

                menu.HasEntry(300)
                    .HasLink("Contest", "Submissions", "List")
                    .HasTitle("fas fa-file-code", "Submissions")
                    .ActiveWhenController("Submissions");

                menu.HasEntry(400)
                    .HasLink("Contest", "Rejudgings", "List")
                    .HasTitle("fas fa-sync-alt", "Rejudgings")
                    .HasIdentifier("menu_rejudgings")
                    .HasBadge("rejudgings", BootstrapColor.info)
                    .ActiveWhenController("Rejudgings");

                menu.HasEntry(500)
                    .HasLink("Contest", "Jury", "Scoreboard")
                    .HasTitle("fas fa-list-ol", "Scoreboard")
                    .ActiveWhenAction("Scoreboard")
                    .RequireThat(ctx => Feature(ctx).Kind == 0);

                menu.HasEntry(600)
                    .HasLink("Contest", "Team", "Home")
                    .HasTitle("fas fa-arrow-right", "Team")
                    .RequireThat(ctx => Feature(ctx).Kind == 0 && Feature(ctx).HasTeam);

                menu.HasEntry(601)
                    .HasLink("Contest", "Gym", "Home")
                    .HasTitle("fas fa-arrow-right", "Gym")
                    .RequireThat(ctx => Feature(ctx).Kind == 1);
            });

            menus.Menu(CcsDefaults.GymNavbar, menu =>
            {
                menu.HasEntry(100)
                    .HasLink("Contest", "Gym", "Home")
                    .HasTitle("fas fa-book-open", "Problems")
                    .ActiveWhenAction("Home,ProblemView");

                menu.HasEntry(200)
                    .HasLink("Contest", "Gym", "Submissions")
                    .HasTitle("fas fa-file-code", "Status")
                    .ActiveWhenAction("Submissions");

                menu.HasEntry(500)
                    .HasLink("Contest", "Gym", "Scoreboard")
                    .HasTitle("fas fa-list-ol", "Standings")
                    .ActiveWhenAction("Scoreboard");

                menu.HasEntry(600)
                    .HasLink("Contest", "Jury", "Home")
                    .HasTitle("fas fa-arrow-right", "Jury")
                    .RequireThat(ctx => Feature(ctx).IsJury);
            });

            menus.Menu(CcsDefaults.PublicNavbar, menu =>
            {
                menu.HasEntry(100)
                    .HasLink("Contest", "Public", "Info")
                    .HasTitle("fas fa-home", "About")
                    .ActiveWhenAction("Info");

                menu.HasEntry(200)
                    .HasLink("Contest", "Public", "Scoreboard")
                    .HasTitle("fas fa-list-ol", "Scoreboard")
                    .ActiveWhenAction("Scoreboard");

                menu.HasEntry(600)
                    .HasLink("Contest", "Jury", "Home")
                    .HasTitle("fas fa-arrow-right", "Jury")
                    .RequireThat(ctx => Feature(ctx).IsJury);

                menu.HasEntry(601)
                    .HasLink("Contest", "Team", "Home")
                    .HasTitle("fas fa-arrow-right", "Team")
                    .RequireThat(ctx => Feature(ctx).HasTeam);
            });

            menus.Menu(CcsDefaults.TeamNavbar, menu =>
            {
                menu.HasEntry(100)
                    .HasLink("Contest", "Team", "Home")
                    .HasTitle("fas fa-home", "Home")
                    .ActiveWhenAction("Home");

                menu.HasEntry(200)
                    .HasLink("Contest", "Team", "Problemset")
                    .HasTitle("fas fa-book-open", "Problemset")
                    .ActiveWhenAction("Problemset");

                menu.HasEntry(400)
                    .HasLink("Contest", "Team", "Print")
                    .HasTitle("fas fa-file-alt", "Print")
                    .ActiveWhenAction("Print")
                    .RequireThat(ctx => Feature(ctx).PrintingAvailable);

                menu.HasEntry(500)
                    .HasLink("Contest", "Team", "Scoreboard")
                    .HasTitle("fas fa-list-ol", "Scoreboard")
                    .ActiveWhenAction("Scoreboard");

                menu.HasEntry(600)
                    .HasLink("Contest", "Jury", "Home")
                    .HasTitle("fas fa-arrow-right", "Jury")
                    .RequireThat(ctx => Feature(ctx).IsJury);
            });

            menus.Component(Polygon.ResourceDictionary.ComponentProblemOverview)
                .HasComponent<Components.ProblemUsage.ProblemUsageViewComponent>(10);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IContestFeature Feature(ViewContext ctx)
            => ctx.HttpContext.Features.Get<IContestFeature>();
    }
}
