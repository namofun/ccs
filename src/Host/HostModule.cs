﻿using Markdig;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite.ContestModule.Routing;
using System.IO;
using System.Threading.Tasks;
using Xylab.Contesting.Connector.Jobs;
using Xylab.Contesting.Registration;
using Xylab.Polygon.Judgement.Daemon.Fake;
using Xylab.Polygon.Storages;

namespace SatelliteSite
{
    public class HostModule : AbstractModule
    {
        public override string Area => string.Empty;

        public override void Initialize()
        {
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapControllers();

            endpoints.MapFallback("/", context =>
            {
                context.Response.Redirect("/dashboard/contests");
                return Task.CompletedTask;
            });
        }

        public override void RegisterServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddMarkdown();

            services.AddDbModelSupplier<DefaultContext, PolygonIdentityEntityConfiguration<MyUser, DefaultContext>>();

            services.ConfigurePolygonStorage(options =>
            {
                options.JudgingDirectory = Path.Combine(environment.ContentRootPath, "Runs");
                options.ProblemDirectory = Path.Combine(environment.ContentRootPath, "Problems");
            });

            services.AddContestRegistrationTenant();

            services.ConfigureApplicationBuilder(options =>
            {
                options.PointBeforeUrlRewriting.Insert(0, app => app.UseMiddleware<Test46160Middleware>());
            });

            services.AddFakeJudgehost()
                .AddFakeSeeds<DefaultContext>()
                .AddJudgehost<FakeJudgeActivity>("fake-judgehost-0")
                .AddHttpClientFactory(_ => new System.Net.Http.HttpClient { BaseAddress = new System.Uri("http://localhost:9121/api/") });

            services.Configure<MinimalSiteOptions>(options =>
            {
                options.Keyword = "hahaahahahahha";
                options.RealIpHeaderName = "Jluds-hhh";
            });

            services.Configure<ExportPdfOptions>(options =>
            {
                options.Url = "http://localhost:43982/api/render";
            });
        }
    }
}
