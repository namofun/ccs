using Ccs;
using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite.ContestModule.Routing;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule
{
    public class ContestModule<TRole> : AbstractModule, IAuthorizationPolicyRegistry
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

            endpoints.WithErrorHandler("Contest", "DomPublic")
                .MapStatusCode("/contest/{cid:c(1)}/{**slug}");

            endpoints.WithErrorHandler("Contest", "Gym")
                .MapStatusCode("/contest/{cid:c(2)}/{**slug}");

            endpoints.WithErrorHandler("Contest", "Problemset")
                .MapStatusCode("/contest/{cid:c(4)}/{**slug}")
                .MapStatusCode("/problemset/{cid:c(4)}/{**slug}");
        }

        public override void RegisterServices(IServiceCollection services)
        {
            new TRole().Configure(services);

            services.AddScoped<ScopedContestContextFactory>();
            services.EnsureSingleton<IContestContextFactory>();
            services.EnsureScoped<IScoreboard>();
            services.EnsureScoped<IPrintingService>();
            services.EnsureScoped<IContestRepository>();

            services.AddContestRegistration();

            services.AddScoped<ContestFeature>();
            services.AddScopedUpcast<IContestContextAccessor, ContestFeature>();
            services.AddScopedUpcast<IContestFeature, ContestFeature>();

            services.AddSingleton<IAuthorizationHandler, ContestAuthorizationHandler>();
            services.AddSingleton<IRewriteRule, ContestOnlyRewriteRule>();
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

            services.ConfigureApplicationCookie(options =>
            {
                var original = options.Events.OnRedirectToAccessDenied;
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    var feature = context.HttpContext.Features.Get<IContestFeature>();
                    if (feature?.Context == null || !(feature.HasTeam || feature.IsJury || feature.IsPublic))
                        return original(context);
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                };
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
                    .HasLink("Contest", "JuryTeams", "List")
                    .HasTitle("fas fa-address-book", "Teams")
                    .HasIdentifier("menu_teams")
                    .HasBadge("teams", BootstrapColor.warning)
                    .ActiveWhenController("JuryTeams");

                menu.HasEntry(200)
                    .HasLink("Contest", "JuryClarifications", "List")
                    .HasTitle("fas fa-comments", "Clarifications")
                    .HasIdentifier("menu_clarifications")
                    .HasBadge("clarifications", BootstrapColor.info)
                    .ActiveWhenController("JuryClarifications");

                menu.HasEntry(300)
                    .HasLink("Contest", "JurySubmissions", "List")
                    .HasTitle("fas fa-file-code", "Submissions")
                    .ActiveWhenController("JurySubmissions");

                menu.HasEntry(400)
                    .HasLink("Contest", "JuryRejudgings", "List")
                    .HasTitle("fas fa-sync-alt", "Rejudgings")
                    .HasIdentifier("menu_rejudgings")
                    .HasBadge("rejudgings", BootstrapColor.info)
                    .ActiveWhenController("JuryRejudgings");

                menu.HasEntry(500)
                    .HasLink("Contest", "Jury", "Scoreboard")
                    .HasTitle("fas fa-list-ol", "Scoreboard")
                    .ActiveWhenAction("Scoreboard")
                    .RequireThat(ctx => Feature(ctx).Kind == 0);

                menu.HasEntry(600)
                    .HasLink("Contest", "DomTeam", "Home")
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
                    .HasLink("Contest", "Gym", "Standings")
                    .HasTitle("fas fa-list-ol", "Standings")
                    .ActiveWhenAction("Standings");

                menu.HasEntry(600)
                    .HasLink("Contest", "Jury", "Home")
                    .HasTitle("fas fa-arrow-right", "Jury")
                    .RequireThat(ctx => Feature(ctx).IsJury);
            });

            menus.Menu(CcsDefaults.PublicNavbar, menu =>
            {
                menu.HasEntry(100)
                    .HasLink("Contest", "DomPublic", "Info")
                    .HasTitle("fas fa-home", "About")
                    .ActiveWhenAction("Info");

                menu.HasEntry(200)
                    .HasLink("Contest", "DomPublic", "Scoreboard")
                    .HasTitle("fas fa-list-ol", "Scoreboard")
                    .ActiveWhenAction("Scoreboard");

                menu.HasEntry(600)
                    .HasLink("Contest", "Jury", "Home")
                    .HasTitle("fas fa-arrow-right", "Jury")
                    .RequireThat(ctx => Feature(ctx).IsJury);

                menu.HasEntry(601)
                    .HasLink("Contest", "DomTeam", "Home")
                    .HasTitle("fas fa-arrow-right", "Team")
                    .RequireThat(ctx => Feature(ctx).HasTeam);
            });

            menus.Menu(CcsDefaults.TeamNavbar, menu =>
            {
                menu.HasEntry(100)
                    .HasLink("Contest", "DomTeam", "Home")
                    .HasTitle("fas fa-home", "Home")
                    .ActiveWhenAction("Home");

                menu.HasEntry(200)
                    .HasLink("Contest", "DomTeam", "ProblemList")
                    .HasTitle("fas fa-book-open", "Problemset")
                    .ActiveWhenAction("ProblemList,ProblemView");

                menu.HasEntry(400)
                    .HasLink("Contest", "DomTeam", "Print")
                    .HasTitle("fas fa-file-alt", "Print")
                    .ActiveWhenAction("Print")
                    .RequireThat(ctx => Feature(ctx).Settings.PrintingAvailable);

                menu.HasEntry(500)
                    .HasLink("Contest", "DomTeam", "Scoreboard")
                    .HasTitle("fas fa-list-ol", "Scoreboard")
                    .ActiveWhenAction("Scoreboard");

                menu.HasEntry(600)
                    .HasLink("Contest", "Jury", "Home")
                    .HasTitle("fas fa-arrow-right", "Jury")
                    .RequireThat(ctx => Feature(ctx).IsJury);
            });

            menus.Menu(CcsDefaults.ProblemsetNavbar, menu =>
            {
                menu.HasEntry(100)
                    .HasLink("Contest", "Problemset", "List")
                    .HasTitle("fas fa-book-open", "Problemset")
                    .ActiveWhenAction("List");

                menu.HasEntry(600)
                    .HasLink("Contest", "Jury", "Home")
                    .HasTitle("fas fa-arrow-right", "Jury")
                    .RequireThat(ctx => Feature(ctx).IsJury);
            });

            menus.Component(Polygon.ResourceDictionary.ComponentProblemOverview)
                .HasComponent<Components.ProblemUsage.ProblemUsageViewComponent>(10);

            menus.Component(IdentityModule.ExtensionPointDefaults.DashboardUserDetail)
                .HasComponent<Components.ParticipantDashboard.ParticipantDashboardViewComponent>(100);
        }

        public void RegisterPolicies(IAuthorizationPolicyContainer container)
        {
            var jury = new ContestJuryRequirement();
            var team = new ContestTeamRequirement();
            var visible = new ContestVisibleRequirement();

            container.AddPolicy("ContestIsJury", b => b.AddRequirements(jury));
            container.AddPolicy("ContestVisible", b => b.AddRequirements(visible));
            container.AddPolicy("ContestHasTeam", b => b.AddRequirements(team));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IContestFeature Feature(ViewContext ctx)
            => ctx.HttpContext.Features.Get<IContestFeature>();
    }
}
