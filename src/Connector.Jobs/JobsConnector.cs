using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xylab.Workflows.Legacy.Services;

[assembly: AffiliateTo(
    typeof(Xylab.Contesting.Connector.Jobs.JobsConnector),
    typeof(SatelliteSite.ContestModule.ContestModule<>),
    typeof(SatelliteSite.JobsModule.JobsModule<,>))]

namespace Xylab.Contesting.Connector.Jobs
{
    public class JobsConnector : AbstractConnector
    {
        public override string Area => "Contest";

        public override void RegisterServices(IServiceCollection services)
        {
            services.AddJobExecutor<ScoreboardXlsx>();
            services.AddJobExecutor<SubmissionZip>();
            services.AddJobExecutor<TeamReportPdf>();
            services.AddScoped<BackgroundViewRenderService>();
            services.AddHttpClient<RemotePdfGenerationService>();
        }

        public override void RegisterMenu(IMenuContributor menus)
        {
            menus.Component(CcsDefaults.ComponentImportExport)
                .HasComponent<Components.JuryJobs.JuryJobsViewComponent>(200);
        }
    }
}
