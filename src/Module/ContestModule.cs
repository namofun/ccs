using Ccs;
using Ccs.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System;

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

            services.AddTransient<IContestContextAccessor, ContestContextAccessor>();
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
                    .RequireThat(ctx => Contest(ctx).Kind == 0);
            });

            menus.Component(Polygon.ResourceDictionary.ComponentProblemOverview)
                .HasComponent<Components.ProblemUsage.ProblemUsageViewComponent>(10);
        }

        private static Ccs.Entities.Contest Contest(ViewContext ctx)
            => ctx.HttpContext.Features.Get<IContestContext>().Contest;
    }
}
