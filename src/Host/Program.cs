using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SatelliteSite.IdentityModule.Entities;

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
                .AddModule<PolygonModule.PolygonModule<Polygon.DefaultRole<DefaultContext, QueryCache>>>()
                .AddModule<GroupModule.GroupModule<DefaultContext>>()
                .AddModule<StudentModule.StudentModule<MyUser, Role, DefaultContext>>()
                .AddModule<ContestModule.ContestModule<Ccs.RelationalRole<MyUser, Role, DefaultContext>>>()
                .AddModule<PlagModule.PlagModule<Plag.Backend.StorageBackendRole<PdsContext>>>()
                .AddModule<TelemetryModule.TelemetryModule>()
                .AddModule<HostModule>()
                .AddModule<JobsModule.JobsModule<MyUser, DefaultContext>>()
                .AddDatabase<DefaultContext>((c, b) => b.UseSqlServer(c.GetConnectionString("UserDbConnection"), b => b.UseBulk().UseMathExtensions()))
                .AddDatabase<PdsContext>((c, b) => b.UseSqlServer(c.GetConnectionString("PlagDbConnection"), b => b.UseBulk()))
                .ConfigureSubstrateDefaults<DefaultContext>();
    }
}
