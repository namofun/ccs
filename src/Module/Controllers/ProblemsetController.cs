using Ccs;
using Ccs.Entities;
using Ccs.Registration;
using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SatelliteSite.ContestModule.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Route("[controller]/{cid:c(4)}")]
    [Authorize(Policy = "ContestVisible")]
    [SupportStatusCodePage]
    public class ProblemsetController : ContestControllerBase<IProblemsetContext>
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewData["BigTitle"] = Contest.ShortName;
            ViewData["NavbarName"] = CcsDefaults.NavbarProblemset;
            ViewData["BigUrl"] = Url.Action("List", "Problemset");
            base.OnActionExecuting(context);
        }


        [HttpGet]
        public async Task<IActionResult> List(int page = 1)
        {
            if (page < 1) return BadRequest();
            var model = await Context.ListProblemsAsync(page, 50);
            ViewBag.Statistics = await Context.StatisticsAsync(Contest.Team);
            ViewBag.GlobalStatistics = await Context.StatisticsGlobalAsync();
            return View(model);
        }


        [HttpGet("{probid}")]
        [ActionName("View")]
        public async Task<IActionResult> ViewProblem(string probid)
        {
            var prob = await Context.FindProblemAsync(probid, true);
            if (prob == null) return NotFound();

            if (string.IsNullOrEmpty(prob.Statement))
            {
                StatusMessage = "Error no descriptions avaliable now.";
                return RedirectToAction(nameof(List));
            }

            return View(prob);
        }


        [HttpGet("{probid}/[action]")]
        public async Task<IActionResult> Submissions(string probid, int draw, int start, int length)
        {
            const int PageCount = 15;
            var prob = await Context.FindProblemAsync(probid);
            if (prob == null) return NotFound();
            if (Contest.Team == null) return DataTableAjax(Array.Empty<object>(), draw, 0);

            if (length != PageCount || start % PageCount != 0)
                return BadRequest();
            start = start / PageCount + 1;
            int teamid = Contest.Team.TeamId, cid = Contest.Id, probId = prob.ProblemId;

            var model = await Context.ListSolutionsAsync(
                selector: (s, j) => new { s.Id, s.Time, s.Language, j.Status, j.TotalScore },
                predicate: s => s.ProblemId == probId && s.TeamId == teamid && s.ContestId == cid,
                page: start, perpage: PageCount);

            return DataTableAjax(model, draw, model.TotalCount);
        }


        [HttpGet("{probid}/submissions/{submitid}")]
        [Authorize(Policy = "ContestHasTeam")]
        public async Task<IActionResult> Submission(string probid, int submitid)
        {
            var prob = await Context.FindProblemAsync(probid);
            if (prob == null) return NotFound();

            int cid = Contest.Id, teamid = Contest.Team.TeamId, probId = prob.ProblemId;
            var subs = await Context.ListSolutionsAsync(
                predicate: s => s.ProblemId == probId && s.TeamId == teamid && s.Id == submitid,
                selector: (s, j) => new CodeViewModel
                {
                    CompileError = j.CompileError,
                    CodeLength = s.CodeLength,
                    ExecuteMemory = j.ExecuteMemory,
                    ExecuteTime = j.ExecuteTime,
                    Code = s.SourceCode,
                    LanguageName = s.l.Name,
                    Status = j.Status,
                    JudgingId = j.Id,
                    SubmissionId = s.Id,
                    DateTime = s.Time,
                    FileExtensions = s.l.FileExtension,
                });

            var sub = subs.SingleOrDefault();
            if (sub == null) return NotFound();
            sub.ProblemTitle = prob.Title;
            sub.Details = await Context.GetDetailsAsync(prob.ProblemId, sub.JudgingId);
            return Window(sub);
        }


        [HttpGet("{probid}/[action]")]
        [Authorize(Policy = "ContestHasTeam")]
        public async Task<IActionResult> Submit(string probid)
        {
            var prob = await Context.FindProblemAsync(probid);
            if (prob == null) return NotFound();

            if (!prob.AllowSubmit)
            {
                return Message(
                    title: $"Submit Problem {probid}",
                    message: $"Problem {probid} is not allowed for submitting.",
                    type: BootstrapColor.danger);
            }

            ViewBag.Language = await Context.ListLanguagesAsync();
            return Window(new CodeSubmitModel
            {
                Code = "",
                Language = "cpp",
                ProblemId = prob.ShortName,
            });
        }


        [HttpPost("{probid}/[action]")]
        [Authorize(Policy = "ContestHasTeam")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(string probid, CodeSubmitModel model)
        {
            if (model.ProblemId != probid) return BadRequest();
            var prob = await Context.FindProblemAsync(probid);
            if (prob == null) return NotFound();

            if (!prob.AllowSubmit)
            {
                ModelState.AddModelError("prob::notallow", $"Problem {probid} is not allowed for submitting.");
            }

            // check language blocking
            var langs = await Context.ListLanguagesAsync();
            var lang = langs.FirstOrDefault(a => a.Id == model.Language);
            if (lang == null)
            {
                ModelState.AddModelError("lang::notallow", "You can't submit solutions with this language.");
            }

            if (ModelState.ErrorCount > 0)
            {
                ViewBag.Language = langs;
                return Window(model);
            }
            else
            {
                await Context.SubmitAsync(
                    code: model.Code,
                    language: lang,
                    problem: prob,
                    team: Contest.Team,
                    ipAddr: HttpContext.Connection.RemoteIpAddress,
                    via: "problem-list",
                    username: User.GetUserName());

                return RedirectToAction(nameof(View));
            }
        }


        [HttpGet("[action]")]
        [Authorize]
        public Task<IActionResult> Register()
            => CommonActions.GetRegister(this, nameof(List));


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        [AuditPoint(AuditlogType.Team)]
        [Authorize]
        public Task<IActionResult> Register([RPBinder("Form")] IRegisterProvider provider)
            => CommonActions.PostRegister(this, provider, nameof(List));
    }
}
