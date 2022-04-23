using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SatelliteSite.IdentityModule.Entities;
using Xylab.PlagiarismDetect.Backend;

namespace SatelliteSite
{
    public class Program
    {
        public static IHost Current { get; private set; }

        public static void Main(string[] args)
        {
            Current = CreateHostBuilder(args).Build();
            Current.AutoMigrate<DefaultContext>();
            Current.MigratePolygonV1();
            Current.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .MarkDomain<Program>()
                .AddModule<IdentityModule.IdentityModule<MyUser, Role, DefaultContext>>()
                .EnableIdentityModuleBasicAuthentication()
                .AddModule<PolygonModule.PolygonModule<Xylab.Polygon.DefaultRole<DefaultContext, QueryCache>>>()
                .AddModule<GroupModule.GroupModule<DefaultContext>>()
                .AddModule<StudentModule.StudentModule<MyUser, Role, DefaultContext>>()
                .AddModule<ContestModule.ContestModule<Xylab.Contesting.RelationalRole<MyUser, Role, DefaultContext>>>()
                .AddModule<PlagModule.PlagModule<StorageBackendRole<PdsContext>>>()
                .AddPlagBackgroundService()
                .AddModule<HostModule>()
                .AddModule<JobsModule.JobsModule<MyUser, DefaultContext>>()
                .AddDatabase<DefaultContext>((c, b) => b.UseSqlServer(c.GetConnectionString("UserDbConnection"), b => b.UseBulk().UseMathExtensions()))
                .AddDatabase<PdsContext>((c, b) => b.UseSqlServer(c.GetConnectionString("PlagDbConnection"), b => b.UseBulk()))
                .ConfigureSubstrateDefaults<DefaultContext>();
    }
}
