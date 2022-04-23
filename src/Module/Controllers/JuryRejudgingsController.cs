using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule.Models;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xylab.Contesting;
using Xylab.Contesting.Entities;
using Xylab.Contesting.Events;
using Xylab.Contesting.Services;
using Xylab.Polygon.Entities;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Authorize(Policy = "ContestIsJury")]
    [Route("[area]/{cid:c(3)}/jury/rejudgings")]
    [AuditPoint(AuditlogType.Rejudging)]
    public class JuryRejudgingsController : JuryControllerBase<IRejudgingContext>
    {
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var model = await Context.ListAsync();
            var uids = model.SelectTwo(r => r.OperatedBy, r => r.IssuedBy).NotNulls().Distinct();
            var userNames = await UserManager.FindUserNamesAsync(uids);
            return View(new JuryListRejudgingModel(model) { UserNames = userNames });
        }


        [HttpGet("{rejudgingid}")]
        public async Task<IActionResult> Detail(int rejudgingid)
        {
            var model = await Context.FindAsync(rejudgingid);
            if (model == null) return NotFound();

            var uids = new[] { model.OperatedBy, model.IssuedBy }.NotNulls().Distinct();
            var userNames = await UserManager.FindUserNamesAsync(uids);
            var teamNames = await Context.GetTeamNamesAsync();
            var difference = await Context.ViewAsync(model);

            return View(new JuryViewRejudgingModel
            {
                TeamNames = teamNames,
                Differences = difference,
                Reason = model.Reason,
                Applied = model.Applied,
                EndTime = model.EndTime,
                Id = model.Id,
                StartTime = model.StartTime,
                IssuedBy = userNames.GetValueOrDefault(model.IssuedBy ?? -110),
                OperatedBy = userNames.GetValueOrDefault(model.OperatedBy ?? -110),
            });
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Add()
        {
            return View(await new AddRejudgingModel().LoadAsync(Context));
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddRejudgingModel model)
        {
            if (!ModelState.IsValid) return View(await model.LoadAsync(Context));

            var r = await Context.CreateAsync(new Rejudging
            {
                ContestId = Contest.Id,
                Reason = model.Reason,
                StartTime = DateTimeOffset.Now,
                IssuedBy = int.Parse(User.GetUserId()),
            });

            var tb = model.TimeBefore.TryParseAsTimeSpan(out var timeBefore) && timeBefore.HasValue;
            var ta = model.TimeAfter.TryParseAsTimeSpan(out var timeAfter) && timeAfter.HasValue;
            var tat = ta ? (Contest.StartTime ?? DateTimeOffset.Now) + timeAfter.Value : DateTimeOffset.UnixEpoch;
            var tbt = tb ? (Contest.StartTime ?? DateTimeOffset.Now) + timeBefore.Value : DateTimeOffset.UnixEpoch;
            model.Problems ??= Array.Empty<int>();
            model.Teams ??= Array.Empty<int>();
            model.Languages ??= Array.Empty<string>();
            model.Judgehosts ??= Array.Empty<string>();
            model.Verdicts ??= Array.Empty<Verdict>();

            var cond = Expr
                .Of<Submission, Judging>((s, j) => true)
                .CombineIf(model.Submission.HasValue, (s, j) => s.Id == model.Submission)
                .CombineIf(model.Problems.Length > 0, (s, j) => model.Problems.Contains(s.ProblemId))
                .CombineIf(model.Teams.Length > 0, (s, j) => model.Teams.Contains(s.TeamId))
                .CombineIf(model.Languages.Length > 0, (s, j) => model.Languages.Contains(s.Language))
                .CombineIf(model.Judgehosts.Length > 0, (s, j) => model.Judgehosts.Contains(j.Server))
                .CombineIf(model.Verdicts.Length > 0, (s, j) => model.Verdicts.Contains(j.Status))
                .CombineIf(ta, (s, j) => s.Time >= tat)
                .CombineIf(tb, (s, j) => s.Time <= tbt);

            int tok = await Context.RejudgeAsync(cond, r, fullTest: Contest.RankingStrategy == CcsDefaults.RuleIOI);

            if (tok == 0)
            {
                await Context.DeleteAsync(r);
                StatusMessage = "Error no submissions was rejudged.";
                return RedirectToAction(nameof(List));
            }
            else
            {
                StatusMessage = $"{tok} submissions will be rejudged.";
                await HttpContext.AuditAsync("added", $"{r.Id}", $"with {tok} submissions");
                return RedirectToAction(nameof(Detail), new { rejudgingid = r.Id });
            }
        }


        [HttpGet("{rejudgingid}/[action]")]
        public IActionResult Repeat()
        {
            return AskPost(
                title: "Repeat rejudging",
                message: "This will create a new rejudging with the same submissions as this rejudging.",
                area: "Contest", controller: "JuryRejudgings", action: "Repeat",
                type: BootstrapColor.primary);
        }


        [HttpPost("{rejudgingid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Repeat(int rejudgingid)
        {
            var rej = await Context.FindAsync(rejudgingid);
            if (rej == null) return NotFound();

            if (rej.OperatedBy == null)
            {
                StatusMessage = "Error rejudging has not been finished.";
                return RedirectToAction(nameof(Detail));
            }

            var r2e = await Context.CreateAsync(new Rejudging
            {
                ContestId = Contest.Id,
                StartTime = DateTimeOffset.Now,
                IssuedBy = int.Parse(User.GetUserId()),
                Reason = "repeat: " + rej.Reason,
            });

            int tok = await Context.RejudgeAsync((s, j) => j.RejudgingId == rejudgingid, r2e);

            if (tok == 0)
            {
                await Context.DeleteAsync(r2e);
                StatusMessage = "Error no submissions was rejudged.";
                return RedirectToAction(nameof(Detail));
            }
            else
            {
                StatusMessage = $"{tok} submissions will be rejudged.";
                await HttpContext.AuditAsync("added", $"{r2e.Id}", $"with {tok} submissions");
                return RedirectToAction(nameof(Detail), new { rejudgingid = r2e.Id });
            }
        }


        [HttpPost("{rejudgingid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int rejudgingid)
        {
            var rej = await Context.FindAsync(rejudgingid);
            if (rej == null || rej.EndTime != null) return NotFound();
            await Context.CancelAsync(rej, int.Parse(User.GetUserId()));
            await HttpContext.AuditAsync("cancelled", $"{rejudgingid}");
            return RedirectToAction(nameof(Detail));
        }


        [HttpPost("{rejudgingid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int rejudgingid)
        {
            var rej = await Context.FindAsync(rejudgingid);
            if (rej == null || rej.EndTime != null) return NotFound();

            var pending = await Context.CountJudgingsAsync(
                j => j.RejudgingId == rejudgingid
                    && (j.Status == Verdict.Pending || j.Status == Verdict.Running));

            if (pending > 0)
            {
                StatusMessage = "Error some submissions are not ready.";
                return RedirectToAction(nameof(Detail));
            }

            await Context.ApplyAsync(rej, int.Parse(User.GetUserId()));
            await HttpContext.AuditAsync("applied", $"{rejudgingid}");
            await Mediator.Publish(new ScoreboardRefreshEvent(Context));
            StatusMessage = "Rejudging applied. Scoreboard cache will be refreshed.";
            return RedirectToAction(nameof(Detail));
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> SystemTest()
        {
            if (!CcsDefaults.SupportsRating
                || Contest.RankingStrategy != CcsDefaults.RuleCodeforces
                || Contest.Kind != CcsDefaults.KindDom)
                return NotFound();

            if (Contest.GetState() < ContestState.Ended)
                return Message("Launch system test", "Precheck failed. Contest should be over.", BootstrapColor.danger);

            if (Contest.Settings.SystemTestRejudgingId is int rejudgingid)
                return RedirectToAction(nameof(Detail), new { rejudgingid });

            var prevs = await Context.ListAsync();
            var non_close = prevs.Where(r => !r.Applied.HasValue).Select(r => $"r{r.Id}").ToList();
            if (non_close.Count > 0)
                return Message("Launch system test", $"Precheck failed. Rejudgings {string.Join(',', non_close)} are not closed.", BootstrapColor.danger);

            return AskPost(
                title: "Launch system test",
                message: "Are you sure to run system test? This will cause heavy system load.",
                type: BootstrapColor.warning);
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SystemTest(bool _ = true)
        {
            if (!CcsDefaults.SupportsRating
                || Contest.RankingStrategy != CcsDefaults.RuleCodeforces
                || Contest.Kind != CcsDefaults.KindDom)
                return NotFound();

            var r = await Context.SystemTestAsync(int.Parse(User.GetUserId()));
            if (r.Success)
            {
                if (Contest.Settings.SystemTestRejudgingId == null) StatusMessage = "System test started.";
                return RedirectToAction(nameof(Detail), new { rejudgingid = r.Result.Id });
            }
            else
            {
                StatusMessage = "Error Precheck failed. " + r.Message;
                return RedirectToAction(nameof(List));
            }
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> ApplyRatingChanges(
            [FromServices] IConfigurationRegistry conf)
        {
            var rejudgingid = Contest.Settings.SystemTestRejudgingId;
            if (!CcsDefaults.SupportsRating
                || rejudgingid == null
                || Contest.Settings.RatingChangesApplied == true)
                return NotFound();

            if ((await conf.GetDateTimeOffsetAsync(CcsDefaults.ConfigurationLastRatingChangeTime)) > Contest.StartTime)
            {
                StatusMessage = "A later contest has done some rating changes. The history has been frozen.";
                return RedirectToAction(nameof(Detail), new { rejudgingid });
            }
            else if ((await Context.CountJudgingsAsync(j => j.RejudgingId == rejudgingid && (j.Status == Verdict.Pending || j.Status == Verdict.Running))) > 0)
            {
                StatusMessage = "System test is not finished. Please wait patiently.";
                return RedirectToAction(nameof(Detail), new { rejudgingid });
            }
            else
            {
                return AskPost(
                    title: "Finish system test",
                    message: "Are you sure to apply rating?",
                    type: BootstrapColor.warning);
            }
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyRatingChanges(
            [FromServices] IRatingUpdater ratingUpdater)
        {
            var rejudgingid = Contest.Settings.SystemTestRejudgingId;
            if (!CcsDefaults.SupportsRating
                || rejudgingid == null
                || Contest.Settings.RatingChangesApplied == true)
                return NotFound();

            if ((await Context.CountJudgingsAsync(j => j.RejudgingId == rejudgingid && (j.Status == Verdict.Pending || j.Status == Verdict.Running))) > 0)
            {
                StatusMessage = "System test is not finished. Please wait patiently.";
                return RedirectToAction(nameof(Detail), new { rejudgingid });
            }

            await Context.ApplyRatingChangesAsync();
            StatusMessage = "Rating changes has been applied to participants.";
            return RedirectToAction(nameof(Detail), new { rejudgingid });
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> RollbackRatingChanges(
            [FromServices] IConfigurationRegistry conf)
        {
            if (!CcsDefaults.SupportsRating
                || Contest.Settings.RatingChangesApplied != true)
                return NotFound();

            if ((await conf.GetDateTimeOffsetAsync(CcsDefaults.ConfigurationLastRatingChangeTime)) > Contest.StartTime)
            {
                StatusMessage = "A later contest has done some rating changes. The history has been frozen.";
                return RedirectToAction(nameof(Detail), new { rejudgingid = Contest.Settings.SystemTestRejudgingId });
            }
            else
            {
                return AskPost(
                    title: "Finish system test",
                    message: "Are you sure to rollback rating changes?",
                    type: BootstrapColor.warning);
            }
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RollbackRatingChanges(
            [FromServices] IRatingUpdater ratingUpdater)
        {
            if (!CcsDefaults.SupportsRating
                || Contest.Settings.RatingChangesApplied != true)
                return NotFound();

            await Context.RollbackRatingChangesAsync();
            StatusMessage = "Rating changes has been rolled back.";
            return RedirectToAction(nameof(Detail), new { rejudgingid = Contest.Settings.SystemTestRejudgingId });
        }
    }
}
