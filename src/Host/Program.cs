using Ccs.Registration;
using Markdig;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                .AddModule<IdentityModule.IdentityModule<MyUser, Role, DefaultContext>>()
                .AddModule<PolygonModule.PolygonModule<Polygon.DefaultRole<DefaultContext, QueryCache>>>()
                .AddModule<GroupModule.GroupModule<DefaultContext>>()
                .AddModule<StudentModule.StudentModule<MyUser, Role, DefaultContext>>()
                .AddModule<ContestModule.ContestModule<Ccs.RelationalRole<MyUser, Role, DefaultContext>>>()
                .AddModule<PlagModule.PlagModule<Plag.Backend.StorageBackendRole<PdsContext>>>()
                .AddModule<TelemetryModule.TelemetryModule>()
                .AddModule<HostModule>()
                .AddDatabase<DefaultContext>((c, b) => b.UseSqlServer(c.GetConnectionString("UserDbConnection"), b => b.UseBulk()))
                .AddDatabase<PdsContext>((c, b) => b.UseSqlServer(c.GetConnectionString("PlagDbConnection"), b => b.UseBulk()))
                .ConfigureSubstrateDefaults<DefaultContext>(builder =>
                {
                    builder.ConfigureServices((context, services) =>
                    {
                        services.AddMarkdown();

                        services.AddDbModelSupplier<DefaultContext, Polygon.Storages.PolygonIdentityEntityConfiguration<MyUser, DefaultContext>>();

                        services.ConfigurePolygonStorage(options =>
                        {
                            options.JudgingDirectory = Path.Combine(context.HostingEnvironment.ContentRootPath, "Runs");
                            options.ProblemDirectory = Path.Combine(context.HostingEnvironment.ContentRootPath, "Problems");
                        });

                        services.AddContestRegistrationTenant();

                        services.ConfigureApplicationBuilder(options =>
                        {
                            options.PointBeforeUrlRewriting.Add(app => app.UseMiddleware<Test46160Middleware>());
                        });

                        return;
                        services.AddFakeJudgehost()
                            .AddFakeSeeds<DefaultContext>()
                            .AddJudgehost<FakeJudgeActivity>("fake-judgehost-0")
                            .AddHttpClientFactory(_ => new System.Net.Http.HttpClient { BaseAddress = new System.Uri("http://localhost:9121/api/") });
                    });
                });
    }
}
