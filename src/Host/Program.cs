using Markdig;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polygon.FakeJudgehost;
using SatelliteSite.IdentityModule.Entities;
using System.IO;

namespace SatelliteSite
{
    public class Program
    {
        public static IHost Current { get; private set; }

        public static void Main(string[] args)
        {
            Current = CreateHostBuilder(args).Build();
            Current.AutoMigrate<DefaultContext>();
            Current.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .MarkDomain<Program>()
                .AddModule<IdentityModule.IdentityModule<User, Role, DefaultContext>>()
                .AddModule<PolygonModule.PolygonModule<Polygon.DefaultRole<User, Role, DefaultContext>>>()
                .AddModule<GroupModule.GroupModule<DefaultContext>>()
                .AddModule<ContestModule.ContestModule<User, Role, DefaultContext>>()
                .AddDatabaseMssql<DefaultContext>("UserDbConnection")//, d => d.UseTableSplittingJoinsRemoval())
                .ConfigureSubstrateDefaults<DefaultContext>(builder =>
                {
                    builder.ConfigureServices((context, services) =>
                    {
                        services.AddMarkdown();
                        services.Configure<Polygon.PolygonPhysicalOptions>(options =>
                        {
                            options.JudgingDirectory = Path.Combine(context.HostingEnvironment.ContentRootPath, "Runs");
                            options.ProblemDirectory = Path.Combine(context.HostingEnvironment.ContentRootPath, "Problems");
                        });

                        services.AddFakeJudgehost()
                            .AddFakeSeeds<DefaultContext>()
                            .AddJudgehost<FakeJudgeActivity>("fake-judgehost-0")
                            .AddHttpClientFactory(_ => new System.Net.Http.HttpClient { BaseAddress = new System.Uri("http://localhost:9121/api/") });
                    });
                });
    }
}
