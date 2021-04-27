using Ccs;
using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite;
using SatelliteSite.ContestModule.Routing;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: RoleDefinition(30, "CDS", "cds_api", "CDS API user")]
[assembly: RoleDefinition(31, "ContestCreator", "cont", "Contest Creator")]
[assembly: RoleDefinition(32, "TemporaryTeamAccount", "temp_team", "Temporary Team Account")]

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
            services.EnsureScoped<IRatingUpdater>();
            services.EnsureScoped<IContestRepository2>();

            services.AddContestRegistration();

            services.AddScoped<ContestFeature>();
            services.AddScopedUpcast<IContestContextAccessor, ContestFeature>();
            services.AddScopedUpcast<IContestFeature, ContestFeature>();

            services.AddSingleton<IAuthorizationHandler, ContestAuthorizationHandler>();
            services.AddSingleton<IRewriteRule, ContestOnlyRewriteRule>();
            services.AddMediatRAssembly(typeof(Ccs.Scoreboard.RankingSolver).Assembly);
            services.AddSingleton<IRatingCalculator, Ccs.Scoreboard.Rating.EloRatingCalculator>();
            services.AddSingleton(SeparatedContestListModelComparer.Instance);

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
                    if (feature?.Context == null || !(feature.HasTeam || feature.JuryLevel.HasValue || feature.IsPublic))
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
                    .ActiveWhenController("Contests")
                    .RequireRoles("Administrator,ContestCreator");
            });

            menus.Submenu(MenuNameDefaults.DashboardUsers, menu =>
            {
                menu.HasEntry(150)
                    .HasTitle(string.Empty, "Contests")
                    .HasLink("Dashboard", "Contests", "List")
                    .RequireRoles("Administrator,ContestCreator");
            });

            menus.Submenu(MenuNameDefaults.DashboardDocuments, menu =>
            {
                menu.HasEntry(151)
                    .HasTitle(string.Empty, "ICPC Contest API")
                    .HasLink("/api/doc/ccsapi");
            });

            menus.Menu(CcsDefaults.NavbarJury, menu =>
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
                    .ActiveWhenController("JuryClarifications")
                    .RequireThat(ctx => Feature(ctx).Kind != CcsDefaults.KindProblemset);

                menu.HasEntry(300)
                    .HasLink("Contest", "JurySubmissions", "List")
                    .HasTitle("fas fa-file-code", "Submissions")
                    .ActiveWhenController("JurySubmissions");

                menu.HasEntry(400)
                    .HasLink("Contest", "JuryRejudgings", "List")
                    .HasTitle("fas fa-sync-alt", "Rejudgings")
                    .HasIdentifier("menu_rejudgings")
                    .HasBadge("rejudgings", BootstrapColor.info)
                    .ActiveWhenController("JuryRejudgings")
                    .RequireThat(ctx => Feature(ctx).Kind != CcsDefaults.KindProblemset);

                menu.HasEntry(500)
                    .HasLink("Contest", "Jury", "Scoreboard")
                    .HasTitle("fas fa-list-ol", "Scoreboard")
                    .ActiveWhenAction("Scoreboard")
                    .RequireThat(ctx => Feature(ctx).Kind == CcsDefaults.KindDom);

                menu.HasEntry(600)
                    .HasLink("Contest", "DomTeam", "Home")
                    .HasTitle("fas fa-arrow-right", "Team")
                    .RequireThat(ctx => Feature(ctx).Kind == CcsDefaults.KindDom && Feature(ctx).HasTeam);

                menu.HasEntry(601)
                    .HasLink("Contest", "Gym", "Home")
                    .HasTitle("fas fa-arrow-right", "Gym")
                    .RequireThat(ctx => Feature(ctx).Kind == CcsDefaults.KindGym);

                menu.HasEntry(602)
                    .HasLink("Contest", "Problemset", "List")
                    .HasTitle("fas fa-arrow-right", "Problemset")
                    .RequireThat(ctx => Feature(ctx).Kind == CcsDefaults.KindProblemset);
            });

            menus.Menu(CcsDefaults.NavbarGym, menu =>
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

            menus.Menu(CcsDefaults.NavbarPublic, menu =>
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

            menus.Menu(CcsDefaults.NavbarTeam, menu =>
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

            menus.Menu(CcsDefaults.NavbarProblemset, menu =>
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

            menus.Menu(CcsDefaults.JuryMenuList, menu =>
            {
                menu.HasSubmenu(0, menu =>
                {
                    menus.Store.Add(CcsDefaults.JuryMenuBefore, menu);

                    menu.HasTitle(string.Empty, "Before Contest")
                        .HasLink("javascript:;");
                });

                menu.HasSubmenu(100, menu =>
                {
                    menus.Store.Add(CcsDefaults.JuryMenuDuring, menu);

                    menu.HasTitle(string.Empty, "During Contest")
                        .HasLink("javascript:;");
                });

                menu.HasSubmenu(200, menu =>
                {
                    menus.Store.Add(CcsDefaults.JuryMenuAdmin, menu);

                    menu.HasTitle(string.Empty, "Administrator")
                        .HasLink("javascript:;");
                });
            });

            menus.Submenu(CcsDefaults.JuryMenuBefore, menu =>
            {
                menu.HasEntry(50)
                    .HasTitle(string.Empty, "Executables")
                    .HasLink("Dashboard", "Executables", "List")
                    .RequireRoles("Administrator");

                menu.HasEntry(100)
                    .HasTitle(string.Empty, "Judgehosts")
                    .HasLink("Dashboard", "Judgehosts", "List")
                    .RequireRoles("Administrator");

                menu.HasEntry(150)
                    .HasTitle(string.Empty, "Languages")
                    .HasLink("Contest", "JuryLanguages", "List");

                menu.HasEntry(200)
                    .HasTitle(string.Empty, "Problems")
                    .HasLink("Contest", "JuryProblems", "List");

                menu.HasEntry(250)
                    .HasTitle(string.Empty, "Teams")
                    .HasLink("Contest", "JuryTeams", "List");

                menu.HasEntry(300)
                    .HasTitle(string.Empty, "Team Categories")
                    .HasLink("Contest", "JuryCategories", "List");

                menu.HasEntry(350)
                    .HasTitle(string.Empty, "Team Affiliations")
                    .HasLink("Contest", "JuryAffiliations", "List");
            });

            menus.Submenu(CcsDefaults.JuryMenuDuring, menu =>
            {
                menu.HasEntry(50)
                    .HasTitle(string.Empty, "Balloon Status")
                    .HasLink("Contest", "JuryBalloons", "List")
                    .RequireThat(c => Feature(c).Settings.BalloonAvailable);

                menu.HasEntry(100)
                    .HasTitle(string.Empty, "Clarifications")
                    .HasLink("Contest", "JuryClarifications", "List")
                    .RequireThat(ctx => Feature(ctx).Kind != CcsDefaults.KindProblemset);

                menu.HasEntry(150)
                    .HasTitle(string.Empty, "Internal Errors")
                    .HasLink("Dashboard", "InternalErrors", "List")
                    .RequireRoles("Administrator");

                menu.HasEntry(200)
                    .HasTitle(string.Empty, "Print")
                    .HasLink("Contest", "Jury", "Print")
                    .RequireThat(c => Feature(c).Settings.PrintingAvailable);

                menu.HasEntry(201)
                    .HasTitle(string.Empty, "Printing Status")
                    .HasLink("Contest", "JuryPrintings", "List")
                    .RequireThat(c => Feature(c).Settings.PrintingAvailable);

                menu.HasEntry(250)
                    .HasTitle(string.Empty, "Rejudgings")
                    .HasLink("Contest", "JuryRejudgings", "List")
                    .RequireThat(ctx => Feature(ctx).Kind != CcsDefaults.KindProblemset);

                menu.HasEntry(300)
                    .HasTitle(string.Empty, "Scoreboard")
                    .HasLink("Contest", "Jury", "Scoreboard")
                    .RequireThat(c => Feature(c).Kind == CcsDefaults.KindDom);

                menu.HasEntry(350)
                    .HasTitle(string.Empty, "Statistics/Analysis")
                    .HasLink("Contest", "JuryAnalysis", "Overview")
                    .RequireThat(c => Feature(c).Kind != CcsDefaults.KindProblemset);

                menu.HasEntry(400)
                    .HasTitle(string.Empty, "Submissions")
                    .HasLink("Contest", "JurySubmissions", "List");
            });

            menus.Submenu(CcsDefaults.JuryMenuAdmin, menu =>
            {
                menu.HasEntry(50)
                    .HasTitle(string.Empty, "Config checker")
                    .HasLink("javascript:alert('oh...');");

                menu.HasEntry(100)
                    .HasTitle(string.Empty, "Refresh scoreboard cache")
                    .HasLink("Contest", "Jury", "RefreshCache");

                menu.HasEntry(102)
                    .HasTitle(string.Empty, "Reset event feed")
                    .HasLink("Contest", "Jury", "ResetEventFeed")
                    .RequireThat(c => Feature(c).Kind == CcsDefaults.KindDom && Feature(c).Settings.EventAvailable);

                menu.HasEntry(150)
                    .HasTitle(string.Empty, "Audit log")
                    .HasLink("Contest", "Jury", "Auditlog");

                menu.HasEntry(200)
                    .HasTitle(string.Empty, "Generate statement LaTeX")
                    .HasLink("Contest", "JuryProblems", "GenerateStatement")
                    .RequireThat(c => Feature(c).Kind != CcsDefaults.KindProblemset);

                menu.HasEntry(350)
                    .HasTitle(string.Empty, "Import / export")
                    .HasLink("Contest", "Jury", "ImportExport")
                    .RequireThat(c => Feature(c).Kind != CcsDefaults.KindProblemset);
            });

            menus.Component(Polygon.ResourceDictionary.ComponentProblemOverview)
                .HasComponent<Components.ProblemUsage.ProblemUsageViewComponent>(10);

            menus.Component(IdentityModule.ExtensionPointDefaults.DashboardUserDetail)
                .HasComponent<Components.ParticipantDashboard.ParticipantDashboardViewComponent>(100);

            menus.Component(IdentityModule.ExtensionPointDefaults.UserDetail)
                .HasComponent<Components.ContestStatistics.ContestStatisticsViewComponent>(10);

            menus.Component(CcsDefaults.ComponentImportExport);
        }

        public void RegisterPolicies(IAuthorizationPolicyContainer container)
        {
            var balloon = new ContestJuryRequirement(Ccs.Entities.JuryLevel.BalloonRunner);
            var jury = new ContestJuryRequirement(Ccs.Entities.JuryLevel.Jury);
            var admin = new ContestJuryRequirement(Ccs.Entities.JuryLevel.Administrator);
            var team = new ContestTeamRequirement();
            var visible = new ContestVisibleRequirement();

            container.AddPolicy("ContestIsBalloonRunner", b => b.AddRequirements(jury));
            container.AddPolicy("ContestIsJury", b => b.AddRequirements(jury));
            container.AddPolicy("ContestIsAdministrator", b => b.AddRequirements(admin));
            container.AddPolicy("ContestVisible", b => b.AddRequirements(visible));
            container.AddPolicy("ContestHasTeam", b => b.AddRequirements(team));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IContestFeature Feature(ViewContext ctx)
            => ctx.HttpContext.Features.Get<IContestFeature>();
    }
}
