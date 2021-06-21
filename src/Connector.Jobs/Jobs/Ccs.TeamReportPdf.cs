using Ccs.Entities;
using Ccs.Services;
using Jobs.Entities;
using Jobs.Models;
using Jobs.Services;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Connector.Jobs
{
    public class TeamReportPdf : IJobExecutorProvider
    {
        private ControllerActionDescriptor ActionDescriptor { get; set; }

        public string Type => "Ccs.TeamReportPdf";

        public static JobDescription Create(Team team, int ownerid)
        {
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var newFileName = $"Team {team.TeamId}: {team.TeamName}.pdf";
            for (int i = 0; i < invalidChars.Length; i++)
                newFileName = newFileName.Replace(invalidChars[i], '_');

            return new JobDescription
            {
                Arguments = new Record { ContestId = team.ContestId, TeamId = team.TeamId }.ToJson(),
                JobType = "Ccs.TeamReportPdf",
                OwnerId = ownerid,
                SuggestedFileName = newFileName,
            };
        }

        public IJobExecutor Create(IServiceProvider serviceProvider)
        {
            ActionDescriptor ??= serviceProvider
                .GetRequiredService<IActionDescriptorCollectionProvider>()
                .ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>()
                .Where(ad => ad.ControllerTypeInfo == typeof(Controllers.JuryJobsController))
                .Where(ad => ad.ActionName == "TeamReportPreview")
                .Single();

            return new Executor(
                serviceProvider.GetRequiredService<RemotePdfGenerationService>(),
                serviceProvider.GetRequiredService<BackgroundViewRenderService>(),
                serviceProvider.GetRequiredService<ScopedContestContextFactory>(),
                serviceProvider.GetRequiredService<IJobFileProvider>(),
                ActionDescriptor);
        }

        private class Record
        {
            public int ContestId { get; set; }

            public int TeamId { get; set; }
        }

        private class Executor : IJobExecutor
        {
            private readonly RemotePdfGenerationService _pdf;
            private readonly BackgroundViewRenderService _view;
            private readonly ScopedContestContextFactory _factory;
            private readonly IJobFileProvider _file;
            private readonly ControllerActionDescriptor _cad;

            public Executor(
                RemotePdfGenerationService pdf,
                BackgroundViewRenderService view,
                ScopedContestContextFactory factory,
                IJobFileProvider file,
                ControllerActionDescriptor cad)
            {
                _pdf = pdf;
                _view = view;
                _factory = factory;
                _file = file;
                _cad = cad;
            }

            public async Task<JobStatus> ExecuteAsync(string arguments, Guid guid, ILogger logger)
            {
                var args = arguments.AsJson<Record>();
                var context = await _factory.CreateAsync(args.ContestId);

                if (context == null)
                {
                    logger.LogError("Unknown contest ID specified.");
                    return JobStatus.Failed;
                }

                var contest = context.Contest;
                if (contest.Feature == CcsDefaults.KindProblemset
                    || contest.RankingStrategy == CcsDefaults.RuleCodeforces)
                {
                    logger.LogError("Export constraint failed.");
                    return JobStatus.Failed;
                }

                logger.LogInformation("Start generating reports.");
                var report = await context.GenerateReport(args.TeamId);
                if (report == null)
                {
                    logger.LogError("Report generation failed.");
                    return JobStatus.Failed;
                }

                logger.LogInformation("Start generating html.");
                var html = await _view.RenderToStringAsync(_cad, "TeamReportPreview", report);

                logger.LogInformation("Start generating PDF.");
                var pdf = await _pdf.GenerateAsync(html);

                logger.LogInformation("Saving to storage.");
                await _file.WriteBinaryAsync(guid + "/main", pdf);
                return JobStatus.Finished;
            }
        }
    }
}
