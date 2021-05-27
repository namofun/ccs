using Ccs.Connector.Jobs.Models;
using Ccs.Services;
using Jobs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule.Controllers;
using System;
using System.Threading.Tasks;

namespace Ccs.Connector.Jobs.Controllers
{
    [Area("Contest")]
    [Authorize(Policy = "ContestIsJury")]
    [Route("[area]/{cid:c(3)}/jury/import-export/[action]-jobs")]
    public class JuryJobsController : JuryControllerBase<IContestContext>
    {
        private readonly IJobScheduler _scheduler;
        public JuryJobsController(IJobScheduler scheduler)
            => _scheduler = scheduler;


        [HttpPost]
        public async Task<IActionResult> ScoreboardXlsx(
            [FromQuery] int[] affiliations = null,
            [FromQuery] int[] categories = null)
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
    }
}
