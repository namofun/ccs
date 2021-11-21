using Ccs.Registration;
using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SatelliteSite.ContestModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Route("[area]/{cid:c(2)}")]
    [Authorize(Policy = "ContestVisible")]
    [SupportStatusCodePage]
    public class GymController : ContestControllerBase<IGymContext>
    {
        private IReadOnlyDictionary<int, (int Accepted, int Total)> Statistics { get; set; }

        private RedirectToActionResult GoBackHome(string message, string action = nameof(Home))
        {
            StatusMessage = message;
            return RedirectToAction(action);
        }

        private ViewResult NotStarted() => View("NotStarted");

        public override async Task OnActionExecutingAsync(ActionExecutingContext context)
        {
            ViewData["NavbarName"] = Ccs.CcsDefaults.NavbarGym;
            ViewData["BigUrl"] = Url.Action("Home", "Gym");
            ViewData["UseLightTheme"] = true;
            await base.OnActionExecutingAsync(context);

            if (context.Result == null)
            {
                Statistics = await Context.StatisticsAsync(Contest.Team);
                ViewData["SubmissionStatistics"] = Statistics;
            }
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Standings(int page = 1)
        {
            if (TooEarly && !Contest.IsJury) return NotStarted();
            if (Contest.ShouldScoreboardPaging() && page <= 0) return BadRequest();
            var scb = await Context.GetScoreboardAsync();
            var members = await Context.GetTeamMembersV2Async();
            return View(new GymStandingViewModel(scb, members) { Page = page });
        }


        [HttpGet]
        public async Task<IActionResult> Home()
        {
            if (TooEarly && !Contest.IsJury) return NotStarted();

            int teamid = Contest.Team?.TeamId ?? -100;
            return View(new GymHomeViewModel
            {
                Clarifications = await Context.ListClarificationsAsync(
                    c => (c.Sender == null && c.Recipient == null)
                      || c.Recipient == teamid || c.Sender == teamid),
                Markdown = await Context.GetReadmeAsync(),
                MeStatistics = Statistics,
                AllStatistics = await Context.StatisticsGlobalAsync(),
                Problems = await Context.ListProblemsAsync(),
            });
        }


        [HttpGet("submissions/{sid}")]
        public async Task<IActionResult> Submission(int sid)
        {
            if (TooEarly && !Contest.IsJury) return NotFound();
            if (!Contest.HasTeam && Contest.Settings.StatusAvailable != 1) return Forbid();

            var model = await Context.FindSolutionAsync(
                sid, (s, j) => new SubmissionViewModel
                {
                    SubmissionId = s.Id,
                    Verdict = j.Status,
                    Time = s.Time,
                    Skipped = s.Ignored,
                    ProblemId = s.ProblemId,
                    CompilerOutput = j.CompileError,
                    LanguageId = s.Language,
                    SourceCode = s.SourceCode,
                    Points = j.TotalScore ?? 0,
                    TeamId = s.TeamId,
                    JudgingId = j.Id,
                    ExecuteMemory = j.ExecuteMemory,
                    ExecuteTime = j.ExecuteTime,
                });

            if (model == null) return NotFound();
            model.Problem = await Context.FindProblemAsync(model.ProblemId);
            model.Language = await Context.FindLanguageAsync(model.LanguageId, contestFiltered: false);

            if (model.Problem == null
                || model.Language == null
                || !Contest.ShouldSubmissionAvailable(
                    model.TeamId == Contest.Team?.TeamId,
                    Statistics.GetValueOrDefault(model.ProblemId).Accepted != 0))
            {
                return Forbid();
            }

            var teamNames = await Context.GetTeamNamesAsync();
            model.TeamName = teamNames.GetValueOrDefault(model.TeamId, "");
            model.Runs = await Context.GetDetailsAsync(model.ProblemId, model.JudgingId);
            if (!model.Problem.Shared) model.Runs = model.Runs.Where(t => !t.Item2.IsSecret);
            return Window(model);
        }


        [HttpGet("problems/{prob}")]
        public async Task<IActionResult> ProblemView(string prob)
        {
            if (TooEarly && !Contest.IsJury) return NotFound();
            var problem = await Context.FindProblemAsync(prob, true);
            if (problem == null) return NotFound();
            ViewBag.CurrentProblem = problem;

            var view = problem.Statement;
            if (string.IsNullOrEmpty(view)) return NotFound();
            ViewData["Content"] = view;

            var model = await Context.ListSolutionsAsync(problem.ProblemId, null, Contest.Team?.TeamId ?? -1000, true);
            return View(model);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Submit(string prob)
        {
            if (!Contest.HasTeam)
            {
                return Message("Submit", "You must have a team first.");
            }
            else if (!Contest.IsTeamAccepted)
            {
                return Message("Submit", "Your team should be verified first. Please contact a staff for this.");
            }
            else if (TooEarly && !Contest.IsJury)
            {
                return Message("Submit", "Contest not started.", BootstrapColor.danger);
            }
            else
            {
                return Window(new TeamCodeSubmitModel
                {
                    Problem = prob,
                    Languages = await Context.ListLanguagesAsync(),
                    Problems = await Context.ListProblemsAsync(),
                });
            }
        }


        [HttpGet("[action]")]
        [Authorize]
        public Task<IActionResult> Register()
            => CommonActions.GetRegister(this, nameof(Home));


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        [AuditPoint(AuditlogType.Team)]
        [Authorize]
        public Task<IActionResult> Register([RPBinder("Form")] IRegisterProvider provider)
            => CommonActions.PostRegister(this, provider, nameof(Home));


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(TeamCodeSubmitModel model)
        {
            if (!Contest.HasTeam)
            {
                ModelState.AddModelError("NoRegistration", "You should register first.");
            }

            if (!Contest.IsTeamAccepted)
            {
                ModelState.AddModelError("NotAccepted", "Team is not accepted yet.");
            }

            if (TooEarly && !Contest.IsJury)
            {
                ModelState.AddModelError("TimeSequence", "Contest not started.");
            }

            var prob = await Context.FindProblemAsync(model.Problem);
            if (prob is null || !prob.AllowSubmit)
            {
                ModelState.AddModelError(nameof(model.Problem), "Problem not found.");
            }

            var lang = await Context.FindLanguageAsync(model.Language);
            if (lang == null)
            {
                ModelState.AddModelError(nameof(model.Language), "Language not found.");
            }

            if (!ModelState.IsValid)
            {
                model.Languages = await Context.ListLanguagesAsync();
                model.Problems = await Context.ListProblemsAsync();
                return Window(model);
            }

            var s = await Context.SubmitAsync(
                code: model.Code,
                language: lang,
                problem: prob,
                team: Contest.Team,
                ipAddr: HttpContext.Connection.RemoteIpAddress,
                via: "gym-page",
                username: User.GetUserName());

            StatusMessage = "Submission done!";
            return RedirectToAction(nameof(ProblemView), new { prob = prob.ShortName });
        }


        [HttpGet("problems/{prob}/testcase/{tcid}/fetch/{filetype}")]
        public async Task<IActionResult> FetchTestcase(string prob, int tcid, string filetype)
        {
            if (filetype == "input") filetype = "in";
            else if (filetype == "output") filetype = "out";
            else return NotFound();

            if (TooEarly && !Contest.IsJury) return NotFound();
            var problem = await Context.FindProblemAsync(prob);
            if (problem == null || !problem.Shared) return NotFound();

            var file = await Context.GetTestcaseAsync(problem, tcid, filetype);
            if (file == null || !file.Exists) return NotFound();

            return File(
                fileStream: file.CreateReadStream(),
                contentType: "application/octet-stream",
                fileDownloadName: $"{problem.ShortName}.t{tcid}.{filetype}");
        }


        private async Task<IActionResult> DoSubmissions(int page, int? verd, string lang, string prob, int? teamid, int filter, int reset)
        {
            if (TooEarly && !Contest.IsJury) return NotStarted();
            if (page <= 0) return BadRequest();

            var verd2 = (Polygon.Entities.Verdict?)verd;
            if (filter != 1 || reset == 1)
            {
                verd2 = null;
                lang = null;
                prob = null;
            }

            ViewBag.Filters = new GymFilteringModel
            {
                Language = lang,
                Problem = prob,
                Verdict = verd2,
            };

            var probs = await Context.ListProblemsAsync();
            var probid = prob != null ? (probs.Find(prob)?.ProblemId ?? -1) : default(int?);
            var model = await Context.ListSolutionsAsync(page, 50, probid, lang, teamid, verd2);
            await Context.ApplyTeamNamesAsync(model);
            return View("Submissions", model);
        }


        [HttpGet("standings/penalty")]
        public async Task<IActionResult> Penalty(int teamid, string probid)
        {
            var team = await Context.FindTeamByIdAsync(teamid);
            var prob = await Context.FindProblemAsync(probid);
            if (team == null || prob == null) return NotFound();

            ViewBag.Team = team;
            ViewBag.Problem = prob;
            var model = await Context.ListSolutionsAsync(probid: prob.ProblemId, teamid: teamid, all: true);
            return View(model);
        }


        [HttpGet("submissions")]
        public Task<IActionResult> Submissions(int page = 1, int? verd = null, string lang = null, string prob = null, int reset = 0, int filter = 0)
            => DoSubmissions(page, verd, lang, prob, null, filter, reset);


        [HttpGet("my-submissions")]
        public Task<IActionResult> MySubmissions(int page = 1, int? verd = null, string lang = null, string prob = null, int reset = 0, int filter = 0)
            => DoSubmissions(page, verd, lang, prob, Contest.Team?.TeamId ?? -100, filter, reset);


        [HttpGet("clarifications/add")]
        public IActionResult ClarificationAdd()
            => CommonActions.ClarificationAdd(this);


        [HttpGet("clarifications/{clarid}")]
        public Task<IActionResult> ClarificationView(int clarid, bool needMore = true)
            => CommonActions.ClarificationView(this, clarid, needMore);


        [HttpPost("clarifications/add")]
        [HttpPost("clarifications/{clarid}/reply")]
        [ValidateAntiForgeryToken]
        [AuditPoint(AuditlogType.Clarification)]
        public Task<IActionResult> ClarificationReply(int? clarid, AddClarificationModel model)
            => CommonActions.ClarificationReply(this, clarid, model, nameof(Home));
    }
}
