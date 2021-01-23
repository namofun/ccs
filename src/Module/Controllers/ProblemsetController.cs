using Ccs;
using Ccs.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataTables;
using Microsoft.AspNetCore.Mvc.Filters;
using SatelliteSite.ContestModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Route("[controller]/{cid:c(4)}")]
    [Authorize(Policy = "ContestVisible")]
    [SupportStatusCodePage]
    public class ProblemsetController : ViewControllerBase
    {
        private IProblemsetContext _context;
        private Team _team;

        private DataTableAjaxResult<T> DataTableAjax<T>(IEnumerable<T> models, int draw, int count)
        {
            return new DataTableAjaxResult<T>(models, draw, count);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var feature = HttpContext.Features.Get<IContestFeature>();
            _context = feature.AsProblemset();
            _team = feature.Team;
            ViewData["BigTitle"] = _context.Contest.ShortName;
            ViewData["NavbarName"] = CcsDefaults.ProblemsetNavbar;
            ViewData["BigUrl"] = Url.Action("List", "Problemset");
            base.OnActionExecuting(context);
        }


        [HttpGet]
        public async Task<IActionResult> List(int page = 1)
        {
            if (page < 1) return BadRequest();
            var model = await _context.ListProblemsetAsync(page, 50);
            ViewBag.Statistics = await _context.StatisticsAsync(_team);
            ViewBag.GlobalStatistics = await _context.StatisticsGlobalAsync();
            return View(model);
        }


        [HttpGet("{probid}")]
        [ActionName("View")]
        public async Task<IActionResult> ViewProblem(string probid)
        {
            var prob = await _context.FindProblemsetAsync(probid, true);
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
            var prob = await _context.FindProblemsetAsync(probid);
            if (prob == null) return NotFound();
            if (_team == null) return DataTableAjax(Array.Empty<object>(), draw, 0);

            if (length != PageCount || start % PageCount != 0)
                return BadRequest();
            start = start / PageCount + 1;
            int teamid = _team.TeamId, cid = _team.ContestId, probId = prob.ProblemId;

            var model = await _context.FetchSolutionsAsync(
                selector: (s, j) => new { s.Id, s.Time, s.Language, j.Status, j.TotalScore },
                predicate: s => s.ProblemId == probId && s.TeamId == teamid && s.ContestId == cid,
                page: start, perpage: PageCount);

            return DataTableAjax(model, draw, model.TotalCount);
        }


        [HttpGet("{probid}/submissions/{submitid}")]
        [Authorize(Policy = "ContestHasTeam")]
        public async Task<IActionResult> Submission(string probid, int submitid)
        {
            var prob = await _context.FindProblemsetAsync(probid);
            if (prob == null) return NotFound();

            int cid = _team.ContestId, teamid = _team.TeamId, probId = prob.ProblemId;
            var subs = await _context.FetchSolutionsAsync(
                predicate: s => s.ProblemId == probId && s.ContestId == cid
                             && s.TeamId == teamid && s.Id == submitid,
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
            sub.Details = await _context.FetchDetailsAsync(prob.ProblemId, sub.JudgingId);
            return Window(sub);
        }


        [HttpGet("{probid}/[action]")]
        [Authorize(Policy = "ContestHasTeam")]
        public async Task<IActionResult> Submit(string probid)
        {
            var prob = await _context.FindProblemsetAsync(probid);
            if (prob == null) return NotFound();

            if (!prob.AllowSubmit)
            {
                return Message(
                    title: $"Submit Problem {probid}",
                    message: $"Problem {probid} is not allowed for submitting.",
                    type: BootstrapColor.danger);
            }

            ViewBag.Language = await _context.FetchLanguagesAsync();
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
            var prob = await _context.FindProblemsetAsync(probid);
            if (prob == null) return NotFound();

            if (!prob.AllowSubmit)
            {
                ModelState.AddModelError("prob::notallow", $"Problem {probid} is not allowed for submitting.");
            }

            // check language blocking
            var langs = await _context.FetchLanguagesAsync();
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
                await _context.SubmitAsync(
                    code: model.Code,
                    language: lang,
                    problem: prob,
                    team: _team,
                    ipAddr: HttpContext.Connection.RemoteIpAddress,
                    via: "problem-list",
                    username: User.GetUserName());

                return RedirectToAction(nameof(View));
            }
        }
    }
}
