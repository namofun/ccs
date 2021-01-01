using Microsoft.AspNetCore.Mvc;
using Polygon.Entities;
using Polygon.Storages;
using SatelliteSite.ContestModule.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Route("[area]/{cid}/jury/[controller]")]
    [AuditPoint(Entities.AuditlogType.Rejudging)]
    public class RejudgingsController : JuryControllerBase
    {
        public IRejudgingStore Store { get; }

        public RejudgingsController(IRejudgingStore store) => Store = store;


        [HttpGet]
        public async Task<IActionResult> List()
        {
            return View(await Store.ListAsync(Contest.Id));
        }


        [HttpGet("{rid}")]
        public async Task<IActionResult> Detail(int rid)
        {
            var model = await Store.FindAsync(Contest.Id, rid);
            if (model == null) return NotFound();
            ViewBag.Teams = await Context.FetchTeamNamesAsync();
            ViewBag.Judgings = await Store.ViewAsync(model);
            return View(model);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Add()
        {
            ViewBag.Teams = await Context.FetchTeamNamesAsync();
            ViewBag.Judgehosts = await Context.GetRequiredService<IJudgehostStore>().ListAsync();
            return View(new AddRejudgingModel());
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddRejudgingModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var r = await Store.CreateAsync(new Rejudging
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
                .Create<Submission, Judging>((s, j) => s.RejudgingId == null && s.ContestId == r.ContestId)
                .CombineIf(model.Submission.HasValue, (s, j) => s.Id == model.Submission)
                .CombineIf(model.Problems.Length > 0, (s, j) => model.Problems.Contains(s.ProblemId))
                .CombineIf(model.Teams.Length > 0, (s, j) => model.Teams.Contains(s.TeamId))
                .CombineIf(model.Languages.Length > 0, (s, j) => model.Languages.Contains(s.Language))
                .CombineIf(model.Judgehosts.Length > 0, (s, j) => model.Judgehosts.Contains(j.Server))
                .CombineIf(model.Verdicts.Length > 0, (s, j) => model.Verdicts.Contains(j.Status))
                .CombineIf(ta, (s, j) => s.Time <= tat)
                .CombineIf(tb, (s, j) => s.Time <= tbt);

            int tok = await Store.BatchRejudgeAsync(cond, r,
                fullTest: Contest.RankingStrategy == 1);

            if (tok == 0)
            {
                await Store.DeleteAsync(r);
                StatusMessage = "Error no submissions was rejudged.";
                return RedirectToAction(nameof(List));
            }
            else
            {
                StatusMessage = $"{tok} submissions will be rejudged.";
                await HttpContext.AuditAsync("added", $"{r.Id}", $"with {tok} submissions");
                return RedirectToAction(nameof(Detail), new { rid = r.Id });
            }
        }


        [HttpGet("{rid}/[action]")]
        public IActionResult Repeat()
        {
            return AskPost(
                title: "Repeat rejudging",
                message: "This will create a new rejudging with the same submissions as this rejudging.",
                area: "Contest", controller: "Rejudgings", action: "Repeat",
                type: BootstrapColor.primary);
        }


        [HttpPost("{rid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Repeat(int rid)
        {
            var rej = await Store.FindAsync(Contest.Id, rid);
            if (rej == null) return NotFound();

            if (rej.OperatedBy == null)
            {
                StatusMessage = "Error rejudging has not been finished.";
                return RedirectToAction(nameof(Detail));
            }

            var r2e = await Store.CreateAsync(new Rejudging
            {
                ContestId = Contest.Id,
                StartTime = DateTimeOffset.Now,
                IssuedBy = int.Parse(User.GetUserId()),
                Reason = "repeat: " + rej.Reason,
            });

            int tok = await Store.BatchRejudgeAsync(
                predicate: (s, j) => j.RejudgingId == rid,
                rejudge: r2e);

            if (tok == 0)
            {
                await Store.DeleteAsync(r2e);
                StatusMessage = "Error no submissions was rejudged.";
                return RedirectToAction(nameof(Detail));
            }
            else
            {
                StatusMessage = $"{tok} submissions will be rejudged.";
                await HttpContext.AuditAsync("added", $"{r2e.Id}", $"with {tok} submissions");
                return RedirectToAction(nameof(Detail), new { rid = r2e.Id });
            }
        }


        [HttpPost("{rid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int rid)
        {
            var rej = await Store.FindAsync(Contest.Id, rid);
            if (rej == null || rej.EndTime != null) return NotFound();
            await Store.CancelAsync(rej, int.Parse(User.GetUserId()));
            await HttpContext.AuditAsync("cancelled", $"{rid}");
            return RedirectToAction(nameof(Detail));
        }


        [HttpPost("{rid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int rid)
        {
            var rej = await Store.FindAsync(Contest.Id, rid);
            if (rej == null || rej.EndTime != null) return NotFound();

            var pending = await Context
                .GetRequiredService<IJudgingStore>()
                .CountAsync(j => j.RejudgingId == rid && (j.Status == Verdict.Pending || j.Status == Verdict.Running));

            if (pending > 0)
            {
                StatusMessage = "Error some submissions are not ready.";
                return RedirectToAction(nameof(Detail));
            }

            await Store.ApplyAsync(rej, int.Parse(User.GetUserId()));
            await HttpContext.AuditAsync("applied", $"{rid}");
            await Mediator.Publish(new Ccs.Events.ScoreboardRefreshEvent(Contest, Problems));
            StatusMessage = "Rejudging applied. Scoreboard cache will be refreshed.";
            return RedirectToAction(nameof(Detail));
        }
    }
}
