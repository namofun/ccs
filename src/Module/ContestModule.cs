using Ccs.Entities;
using Ccs.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite.IdentityModule.Entities;

namespace SatelliteSite.ContestModule
{
    public class ContestModule<TUser, TRole, TContext> : AbstractModule
        where TUser : User, new()
        where TRole : Role, new()
        where TContext : DbContext
    {
        public override string Area => "Contest";

        public override void Initialize()
        {
        }

        public override void RegisterEndpoints(IEndpointBuilder endpoints)
        {
            endpoints.MapControllers();
        }

        public override void RegisterServices(IServiceCollection services)
        {
            services.AddDbModelSupplier<TContext, ContestEntityConfiguration<TUser, TRole, TContext>>();
            services.AddScoped<IContestStore, ContestStore<TContext>>();
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
        }
    }
}
