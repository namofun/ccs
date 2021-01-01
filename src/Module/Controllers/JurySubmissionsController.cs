using Ccs;
using Microsoft.AspNetCore.Mvc;
using Polygon.Entities;
using Polygon.Storages;
using SatelliteSite.ContestModule.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Route("[area]/{cid}/jury/[controller]")]
    public class SubmissionsController : JuryControllerBase
    {
        private ISubmissionStore Store { get; }

        public SubmissionsController(ISubmissionStore store) => Store = store;


        [HttpGet]
        public async Task<IActionResult> List(bool all = false)
        {
            var model = await Context.FetchSolutionsAsync(all: all);
            return View(model);
        }


        [HttpGet("{sid}/{jid?}")]
        public async Task<IActionResult> Detail(int sid, int? jid)
        {
            var submit = await Store.FindAsync(sid, true);
            if (submit == null || submit.ContestId != Contest.Id) return NotFound();
            var judgings = submit.Judgings;

            var prob = Problems.Find(submit.ProblemId);
            if (prob == null) return NotFound(); // the problem is deleted later

            var judging = jid.HasValue
                ? judgings.SingleOrDefault(j => j.Id == jid.Value)
                : judgings.SingleOrDefault(j => j.Active);
            if (judging == null) return NotFound();

            var langs = await Context.FetchLanguagesAsync();
            var store2 = Context.GetRequiredService<IJudgingStore>();
            return View(new JuryViewSubmissionModel
            {
                Submission = submit,
                Judging = judging,
                AllJudgings = judgings,
                DetailsV2 = await store2.GetDetailsAsync(submit.ProblemId, judging.Id),
                Team = await Context.FindTeamByIdAsync(submit.TeamId),
                Problem = prob,
                Language = langs.First(l => l.Id == submit.Language),
            });
        }


        [HttpGet("{sid}/[action]")]
        public async Task<IActionResult> Source(int sid, int? last = null)
        {
            var submit = await Store.FindAsync(sid);
            if (submit == null || submit.ContestId != Contest.Id) return NotFound();

            var cond = Expr
                .Create<Submission>(s => s.ContestId == submit.ContestId
                                      && s.TeamId == submit.TeamId
                                      && s.ProblemId == submit.ProblemId)
                .CombineIf(last.HasValue, s => s.Id == last)
                .CombineIf(!last.HasValue, s => s.Id < sid);
            
            var lastSubmit = (await Store.ListWithJudgingAsync(
                selector: (s, j) => new { s.Language, s.SourceCode, s.Id },
                predicate: cond, 1))
                .FirstOrDefault();

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
            var sub = await Store.FindAsync(sid);
            if (sub == null || sub.ContestId != Contest.Id) return NotFound();

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
