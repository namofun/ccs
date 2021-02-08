using Ccs.Services;
using Microsoft.AspNetCore.Mvc;
using Polygon.Entities;
using SatelliteSite.ContestModule.Models;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Route("[area]/{cid:c(7)}/jury/submissions")]
    public class JurySubmissionsController : JuryControllerBase<ISubmissionContext>
    {
        [HttpGet]
        public async Task<IActionResult> List(bool all = false)
        {
            var model = await Context.ListSolutionsAsync(all: all);
            var teamNames = await Context.GetTeamNamesAsync();
            model.ForEach(a => a.AuthorName = teamNames.GetValueOrDefault(a.TeamId));
            return View(model);
        }


        [HttpGet("{submitid}")]
        public async Task<IActionResult> Detail(int submitid, int? judgingid)
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
                Language = await Context.FindLanguageAsync(submit.Language),
            });
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
                NewLang = await Context.FindLanguageAsync(submit.Language),
                OldCode = lastSubmit?.SourceCode,
                OldId = lastSubmit?.Id,
                OldLang = await Context.FindLanguageAsync(lastSubmit?.Language),
            });
        }


        [HttpGet("{submitid}/[action]")]
        public async Task<IActionResult> Rejudge(int submitid)
        {
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
    }
}
