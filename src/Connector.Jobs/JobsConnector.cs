using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

[assembly: AffiliateTo(
    typeof(Ccs.Connector.Jobs.JobsConnector),
    typeof(SatelliteSite.ContestModule.ContestModule<>),
    typeof(SatelliteSite.JobsModule.JobsModule<,>))]

namespace Ccs.Connector.Jobs
{
    public class JobsConnector : AbstractConnector
    {
        public override string Area => "Contest";

        public override void RegisterServices(IServiceCollection services)
        {
            services.AddJobExecutor<ScoreboardXlsx>();
        }

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Component(CcsDefaults.ComponentImportExport)
                .HasComponent<Components.JuryJobs.JuryJobsViewComponent>(120);
        }
    }
}
