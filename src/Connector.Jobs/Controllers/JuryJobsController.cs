using Ccs.Connector.Jobs.Models;
using Ccs.Models;
using Ccs.Services;
using Jobs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule.Controllers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Connector.Jobs.Controllers
{
    [Area("Contest")]
    [Authorize(Policy = "ContestIsJury")]
    [Route("[area]/{cid:c(3)}/jury/import-export/jobs")]
    public class JuryJobsController : JuryControllerBase<IContestContext>
    {
        private readonly IJobScheduler _scheduler;
        public JuryJobsController(IJobScheduler scheduler)
            => _scheduler = scheduler;


        [HttpPost("[action]")]
        public async Task<IActionResult> ScoreboardXlsx(
            int[] affiliations = null,
            int[] categories = null,
            bool afterend = false)
        {
            if (Contest.RankingStrategy == CcsDefaults.RuleCodeforces) return StatusCode(501);
            if (!Contest.StartTime.HasValue) return BadRequest();
            if (affiliations != null && affiliations.Length == 0) affiliations = null;
            if (categories != null && categories.Length == 0) categories = null;

            var args = new ScoreboardArguments
            {
                FilteredAffiliations = affiliations,
                FilteredCategories = categories,
                ContestId = Contest.Id,
                IncludeUpsolving = afterend,
            };

            var job = await _scheduler.ScheduleAsync(
                Jobs.ScoreboardXlsx.Create(
                    args,
                    int.Parse(User.GetUserId())));

            StatusMessage =
                $"Scoreboard export job scheduled. " +
                $"You can view {job.JobId} later in your export files.";
            return RedirectToAction("ImportExport", "Jury");
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> SubmissionZip(
            int[] affiliations = null,
            int[] categories = null,
            int[] problems = null,
            bool beforestart = false,
            bool during = false,
            bool afterend = false)
        {
            if (!beforestart && !during && !afterend) return BadRequest();
            if (!Contest.StartTime.HasValue) return BadRequest();
            if (affiliations != null && affiliations.Length == 0) affiliations = null;
            if (categories != null && categories.Length == 0) categories = null;
            if (problems != null && problems.Length == 0) problems = null;

            var args = new SubmissionArguments
            {
                FilteredAffiliations = affiliations,
                FilteredCategories = categories,
                ContestId = Contest.Id,
                IncludeUpsolving = afterend,
                IncludeInner = during,
                IncludePrevious = beforestart,
                FilteredProblems = problems,
            };

            var job = await _scheduler.ScheduleAsync(
                Jobs.SubmissionZip.Create(
                    args,
                    int.Parse(User.GetUserId())));

            StatusMessage =
                $"Submission export job scheduled. " +
                $"You can view {job.JobId} later in your export files.";
            return RedirectToAction("ImportExport", "Jury");
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> TeamReport(
            int[] affiliations = null,
            int[] categories = null)
        {
            if (Contest.GetState() < Entities.ContestState.Ended) return BadRequest();
            if (affiliations != null && affiliations.Length == 0) affiliations = null;
            if (categories != null && categories.Length == 0) categories = null;

            var args = new ScoreboardArguments
            {
                FilteredAffiliations = affiliations,
                FilteredCategories = categories,
                ContestId = Contest.Id,
                IncludeUpsolving = false,
            };

            var teams = await ((ITeamContext)Context).ListTeamsAsync(t => t.Status == 1);

            var ownerId = int.Parse(User.GetUserId());
            var jobDesc = global::Jobs.Works.ComposeArchive.ForChildren(
                ownerId,
                $"c{Contest.Id}-team-reports.zip",
                teams
                    .WhereIf(affiliations != null, t => affiliations.Contains(t.AffiliationId))
                    .WhereIf(categories != null, t => categories.Contains(t.CategoryId))
                    .Select(t => TeamReportPdf.Create(t, ownerId))
                    .ToList());

            jobDesc.Arguments = args.ToJson();
            var job = await _scheduler.ScheduleAsync(jobDesc);

            StatusMessage =
                $"PDF export job scheduled. " +
                $"You can view {job.JobId} later in your export files.";
            return RedirectToAction("ImportExport", "Jury");
        }


        [HttpGet("team-report/preview/{teamid}")]
        public async Task<IActionResult> TeamReportPreview(int teamid)
        {
            var report = await Context.GenerateReport(teamid);
            if (report == null) return NotFound();
            return View(report);
        }
    }
}
