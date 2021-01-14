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
    [Route("[area]/{cid}/jury/[controller]")]
    public class SubmissionsController : JuryControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> List(bool all = false)
        {
            var model = await Context.FetchSolutionsAsync(all: all);
            var teamNames = await Context.FetchTeamNamesAsync();
            model.ForEach(a => a.AuthorName = teamNames.GetValueOrDefault(a.TeamId));
            return View(model);
        }


        [HttpGet("{sid}/{jid?}")]
        public async Task<IActionResult> Detail(int sid, int? jid)
        {
            var submit = await Context.FindSubmissionAsync(sid, true);
            if (submit == null) return NotFound();
            var judgings = submit.Judgings;

            var prob = Problems.Find(submit.ProblemId);
            if (prob == null) return NotFound(); // the problem is deleted later

            var judging = jid.HasValue
                ? judgings.SingleOrDefault(j => j.Id == jid.Value)
                : judgings.SingleOrDefault(j => j.Active);
            if (judging == null) return NotFound();

            var langs = await Context.FetchLanguagesAsync();
            return View(new JuryViewSubmissionModel
            {
                Submission = submit,
                Judging = judging,
                AllJudgings = judgings,
                DetailsV2 = await Context.FetchDetailsAsync(submit.ProblemId, judging.Id),
                Team = await Context.FindTeamByIdAsync(submit.TeamId),
                Problem = prob,
                Language = langs.First(l => l.Id == submit.Language),
            });
        }


        [HttpGet("{sid}/[action]")]
        public async Task<IActionResult> Source(int sid, int? last = null)
        {
            int cid = Contest.Id;
            var submit = await Context.FetchSourceAsync(s => s.ContestId == cid && s.Id == sid);
            if (submit == null) return NotFound();

            var cond = Expr
                .Of<Submission>(s => s.ContestId == cid)
                .Combine(s => s.TeamId == submit.TeamId && s.ProblemId == submit.ProblemId)
                .CombineIf(last.HasValue, s => s.Id == last)
                .CombineIf(!last.HasValue, s => s.Id < sid);

            var lastSubmit = await Context.FetchSourceAsync(cond);
            var langs = await Context.FetchLanguagesAsync();

            return View(new SubmissionSourceModel
            {
                ProblemId = submit.ProblemId,
                TeamId = submit.TeamId,
                NewCode = submit.SourceCode,
                NewId = submit.Id,
                NewLang = langs.FirstOrDefault(l => l.Id == submit.Language),
                OldCode = lastSubmit?.SourceCode,
                OldId = lastSubmit?.Id,
                OldLang = langs.FirstOrDefault(l => l.Id == lastSubmit?.Language),
            });
        }


        [HttpGet("{sid}/[action]")]
        public async Task<IActionResult> Rejudge(int sid)
        {
            var sub = await Context.FindSubmissionAsync(sid);
            if (sub == null) return NotFound();

            if (sub.RejudgingId != null)
                return RedirectToAction("Detail", "Rejudgings", new { rid = sub.RejudgingId });

            return Window(new AddRejudgingModel
            {
                Submission = sid,
                Reason = $"submission: {sid}",
            });
        }
    }
}
