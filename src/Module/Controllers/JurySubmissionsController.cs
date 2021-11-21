using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polygon.Entities;
using SatelliteSite.ContestModule.Models;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Authorize(Policy = "ContestIsJury")]
    [Route("[area]/{cid:c(7)}/jury/submissions")]
    public class JurySubmissionsController : JuryControllerBase<ISubmissionContext>
    {
        [HttpGet]
        public async Task<IActionResult> List(int? probid = null, string langid = null, int? teamid = null, bool all = false)
        {
            var model = await Context.ListSolutionsAsync(probid: probid, langid: langid, teamid: teamid, all: all);
            await Context.ApplyTeamNamesAsync(model);
            return View(model);
        }


        [HttpGet("{submitid}")]
        public async Task<IActionResult> Detail(int submitid, int? judgingid = null)
        {
            var submit = await Context.FindSubmissionAsync(submitid, true);
            if (submit == null) return NotFound();
            var judgings = submit.Judgings;

            var prob = await Context.FindProblemAsync(submit.ProblemId);
            if (prob == null) return NotFound(); // the problem is deleted later

            var judging = judgingid.HasValue
                ? judgings.SingleOrDefault(j => j.Id == judgingid.Value)
                : judgings.SingleOrDefault(j => j.Active);
            if (judging == null) return NotFound();

            return View(new JuryViewSubmissionModel
            {
                Submission = submit,
                Judging = judging,
                AllJudgings = judgings,
                DetailsV2 = await Context.GetDetailsAsync(submit.ProblemId, judging.Id),
                Team = await Context.FindTeamByIdAsync(submit.TeamId),
                Problem = prob,
                Language = await Context.FindLanguageAsync(submit.Language, contestFiltered: false),
            });
        }


        [HttpGet("{submitid}/[action]/{judgingid}/{runid}")]
        public async Task<IActionResult> RunDetails(int submitid, int judgingid, int runid)
        {
            var submit = await Context.FindSubmissionAsync(submitid);
            if (submit == null) return NotFound();
            
            var run = await Context.GetDetailAsync(submit.ProblemId, submitid, judgingid, runid);
            if (run == null) return NotFound();

            var prob = await Context.FindProblemAsync(submit.ProblemId);
            ViewBag.CombinedRunCompare = prob?.Interactive ?? false;
            return Window(run);
        }


        [HttpGet("{submitid}/[action]")]
        public async Task<IActionResult> Source(int submitid, int? last = null)
        {
            int cid = Contest.Id;
            var submit = await Context.GetSourceCodeAsync(s => s.ContestId == cid && s.Id == submitid);
            if (submit == null) return NotFound();

            var cond = Expr
                .Of<Submission>(s => s.ContestId == cid)
                .Combine(s => s.TeamId == submit.TeamId && s.ProblemId == submit.ProblemId)
                .CombineIf(last.HasValue, s => s.Id == last)
                .CombineIf(!last.HasValue, s => s.Id < submitid);

            var lastSubmit = await Context.GetSourceCodeAsync(cond);

            return View(new SubmissionSourceModel
            {
                ProblemId = submit.ProblemId,
                TeamId = submit.TeamId,
                NewCode = submit.SourceCode,
                NewId = submit.Id,
                NewLang = await Context.FindLanguageAsync(submit.Language, contestFiltered: false),
                OldCode = lastSubmit?.SourceCode,
                OldId = lastSubmit?.Id,
                OldLang = await Context.FindLanguageAsync(lastSubmit?.Language, contestFiltered: false),
            });
        }


        [HttpGet("{submitid}/[action]")]
        public async Task<IActionResult> Rejudge(int submitid)
        {
            if (Contest.Feature == Ccs.CcsDefaults.KindProblemset)
            {
                return Message(
                    title: "Rejudging",
                    message: "Rejudging not supported in problem sets. Please submit this solution again.");
            }

            var sub = await Context.FindSubmissionAsync(submitid);
            if (sub == null) return NotFound();

            if (sub.RejudgingId != null)
            {
                return RedirectToAction("Detail", "JuryRejudgings", new { rejudgingid = sub.RejudgingId });
            }

            return Window(new AddRejudgingModel
            {
                Submission = submitid,
                Reason = $"submission: {submitid}",
            });
        }


        [HttpGet("{submitid}/[action]")]
        public async Task<IActionResult> Ignore(int submitid)
        {
            var sub = await Context.FindSubmissionAsync(submitid);
            if (sub == null) return NotFound();
            var team = await Context.FindTeamByIdAsync(sub.TeamId);
            var prob = await Context.FindProblemAsync(sub.ProblemId);

            return AskPost(
                title: $"{(sub.Ignored ? "Unignore" : "Ignore")} submission",
                message: $"Are you sure to {(sub.Ignored ? "unignore" : "ignore")} " +
                    $"submission s{submitid} " +
                    $"from team {team?.TeamName} (t{sub.TeamId}) " +
                    $"on problem {prob?.ShortName ?? "?"} - {prob?.Title ?? "???"} (p{sub.ProblemId})?",
                type: BootstrapColor.danger);
        }


        [HttpPost("{submitid}/[action]")]
        [ValidateAntiForgeryToken]
        [AuditPoint(AuditlogType.Submission)]
        public async Task<IActionResult> Ignore(int submitid, bool _ = true)
        {
            var sub = await Context.FindSubmissionAsync(submitid);
            if (sub == null) return NotFound();

            var origIgnore = sub.Ignored;
            await Context.ToggleIgnoreAsync(sub, !origIgnore);
            await HttpContext.AuditAsync(origIgnore ? "unignored" : "ignored", submitid.ToString());
            StatusMessage = $"Submission s{submitid} has been {(origIgnore ? "un" : "")}ignored. You may have to refresh the scoreboard manually.";
            return RedirectToAction(nameof(Detail));
        }
    }
}
