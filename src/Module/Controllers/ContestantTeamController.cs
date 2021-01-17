using Ccs.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SatelliteSite.ContestModule.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Authorize]
    [Route("[area]/{cid:c(1)}")]
    public partial class TeamController : ContestControllerBase
    {
        public bool TooEarly => Contest.GetState() < ContestState.Started;

        protected IActionResult GoBackHome(string message)
        {
            StatusMessage = message;
            return Home();
        }

        public override Task OnActionExecutingAsync(ActionExecutingContext context)
        {
            if (Team == null || Team.Status != 1)
            {
                context.Result = IsWindowAjax
                    ? Message("401 Unauthorized",
                        "This contest is not active for you (yet).",
                        BootstrapColor.danger)
                    : View("AccessDenied");
            }

            ViewData["NavbarName"] = Ccs.CcsDefaults.TeamNavbar;
            ViewData["BigUrl"] = Url.Action("Home", "Team");
            ViewData["ExtraMenu"] = "_NavButton";
            return base.OnActionExecutingAsync(context);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Home()
        {
            if (Team == null) return RedirectToAction("Public");
            return RedirectToAction("Team");
        }


        [HttpGet("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> Public()
        {
            ViewBag.Affiliations = await Context.FetchAffiliationsAsync();
            ViewBag.Categories = await Context.FetchCategoriesAsync();
            ViewBag.Markdown = await Context.GetReadmeAsync();
            return View();
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        [AuditPoint(AuditlogType.Team)]
        public async Task<IActionResult> Register()
        {
            if (ViewData.ContainsKey("HasTeam"))
            {
                StatusMessage = "Already registered";
                return RedirectToAction(nameof(Public));
            }

            if (!Contest.RegisterCategory.HasValue || User.IsInRole("Blocked"))
            {
                StatusMessage = "Error registration closed.";
                return RedirectToAction(nameof(Public));
            }

            string defaultAff = User.IsInRole("Student") ? "jlu" : "null";
            var affiliations = await Context.FetchAffiliationsAsync(false);
            var aff = affiliations.Values.SingleOrDefault(a => a.Abbreviation == defaultAff);
            if (aff == null) throw new ApplicationException("No default affiliation.");

            var user = await UserManager.GetUserAsync(User);

            var team = await Context.CreateTeamAsync(
                users: new[] { user },
                team: new Team
                {
                    AffiliationId = aff.Id,
                    ContestId = Contest.Id,
                    CategoryId = Contest.RegisterCategory.Value,
                    RegisterTime = DateTimeOffset.Now,
                    Status = 0,
                    TeamName = User.GetNickName(),
                });

            await HttpContext.AuditAsync("added", $"{team.TeamId}");
            StatusMessage = "Registration succeeded.";
            return RedirectToAction(nameof(Public));
        }


        [HttpGet("[action]")]
        public new IActionResult Print()
            => base.Print();


        [HttpPost("[action]")]
        [AuditPoint(AuditlogType.Printing)]
        public new Task<IActionResult> Print(AddPrintModel model)
            => base.Print(model);


        [HttpGet("[action]")]
        public Task<IActionResult> Scoreboard(
            [FromQuery(Name = "affiliations[]")] int[] affiliations,
            [FromQuery(Name = "categories[]")] int[] categories,
            [FromQuery(Name = "clear")] string clear = "")
            => Scoreboard(
                isPublic: Contest.GetState() < ContestState.Finalized,
                isJury: false, clear == "clear", affiliations, categories);


        [HttpGet("[action]")]
        [ActionName("Team")]
        public async Task<IActionResult> TeamHome()
        {
            int teamid = Team.TeamId;
            var board = await Context.SingleBoardAsync(teamid);
            
            ViewBag.Clarifications = await Context.ListClarificationsAsync(
                c => (c.Sender == null && c.Recipient == null)
                || c.Recipient == teamid || c.Sender == teamid);

            ViewBag.Submissions = await Context.FetchSolutionsAsync(
                teamid: teamid,
                selector: (s, j) => new SubmissionViewModel
                {
                    Points = j.TotalScore ?? 0,
                    Language = s.Language,
                    SubmissionId = s.Id,
                    Time = s.Time,
                    Verdict = j.Status,
                    Problem = s.ProblemId,
                });

            return View(board);
        }


        [HttpGet("[action]")]
        public IActionResult Problemset()
        {
            return View(Problems);
        }


        [HttpGet("[action]/{sid}")]
        public async Task<IActionResult> Submission(int sid)
        {
            int teamid = Team.TeamId;

            var model = await Context.FetchSolutionAsync(
                sid, (s, j) => new SubmissionViewModel
                {
                    SubmissionId = s.Id,
                    Points = j.TotalScore ?? 0,
                    Language = s.Language,
                    Time = s.Time,
                    Verdict = j.Status,
                    Problem = s.ProblemId,
                    CompilerOutput = j.CompileError,
                    SourceCode = s.SourceCode,
                });

            if (model == null) return NotFound();
            return Window(model);
        }


        [HttpGet("problems/{prob}")]
        public IActionResult Problemset(string prob)
        {
            if (TooEarly && !ViewData.ContainsKey("IsJury")) return NotFound();
            var problem = Problems.Find(prob);
            if (problem == null) return NotFound();

            var view = problem.Statement;
            if (string.IsNullOrEmpty(view)) return NotFound();

            ViewData["Content"] = view;
            return View("ProblemView");
        }


        [HttpGet("clarifications/add")]
        public IActionResult AddClarification()
        {
            if (TooEarly) return Message("Clarification", "Contest has not started.");
            return Window(new AddClarificationModel());
        }


        [HttpGet("clarifications/{clarid}")]
        public async Task<IActionResult> Clarification(int clarid, bool needMore = true)
        {
            var toSee = await Context.FindClarificationAsync(clarid);
            var clars = Enumerable.Empty<Clarification>();

            if (toSee?.CheckPermission(Team.TeamId) ?? true)
            {
                clars = clars.Append(toSee);

                if (needMore && toSee.ResponseToId.HasValue)
                {
                    int respid = toSee.ResponseToId.Value;
                    var toSee2 = await Context.FindClarificationAsync(respid);
                    if (toSee2 != null) clars = clars.Prepend(toSee2);
                }
            }

            if (!clars.Any()) return NotFound();
            ViewData["TeamName"] = Team.TeamName;
            return Window(clars);
        }


        [HttpPost("clarifications/{op}")]
        [ValidateAntiForgeryToken]
        [AuditPoint(AuditlogType.Clarification)]
        public async Task<IActionResult> Clarification(
            string op, AddClarificationModel model)
        {
            var (cid, teamid) = (Contest.Id, Team.TeamId);
            int repl = 0;
            if (op != "add" && !int.TryParse(op, out repl)) return NotFound();

            var replit = await Context.FindClarificationAsync(repl);
            if (repl != 0 && replit == null)
                ModelState.AddModelError("xys::replyto", "The clarification replied to is not found.");

            if (string.IsNullOrWhiteSpace(model.Body))
                ModelState.AddModelError("xys::empty", "No empty clarification");

            var usage = Problems.ClarificationCategories.SingleOrDefault(cp => model.Type == cp.Item1);
            if (usage.Item1 == null)
                ModelState.AddModelError("xys::error_cate", "The category specified is wrong.");

            if (!ModelState.IsValid)
            {
                StatusMessage = string.Join('\n', ModelState.Values
                    .SelectMany(m => m.Errors)
                    .Select(e => e.ErrorMessage));
            }
            else
            {
                var clar = await Context.ClarifyAsync(
                    new Clarification
                    {
                        Body = model.Body,
                        SubmitTime = DateTimeOffset.Now,
                        ContestId = cid,
                        Sender = teamid,
                        ResponseToId = model.ReplyTo,
                        ProblemId = usage.Item3,
                        Category = usage.Item2,
                    });

                await HttpContext.AuditAsync("added", $"{clar.Id}");
                StatusMessage = "Clarification sent to the jury.";
            }

            return RedirectToAction(nameof(Team));
        }


        [HttpGet("[action]")]
        public IActionResult Submit()
        {
            if (TooEarly && !ViewData.ContainsKey("IsJury"))
                return Message("Submit", "Contest not started.", BootstrapColor.danger);
            return Window(new TeamCodeSubmitModel());
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(TeamCodeSubmitModel model)
        {
            if (TooEarly && !ViewData.ContainsKey("IsJury"))
            {
                StatusMessage = "Contest not started.";
                return RedirectToAction(nameof(Team));
            }

            var prob = Problems.Find(model.Problem);
            if (prob is null || !prob.AllowSubmit)
            {
                StatusMessage = "Error problem not found.";
                return RedirectToAction(nameof(Team));
            }

            var langs = await Context.FetchLanguagesAsync();
            var lang = langs.FirstOrDefault(l => l.Id == model.Language);
            if (lang == null)
            {
                StatusMessage = "Error language not found.";
                return RedirectToAction(nameof(Team));
            }

            var s = await Context.SubmitAsync(
                code: model.Code,
                language: lang,
                problem: prob,
                team: Team,
                ipAddr: HttpContext.Connection.RemoteIpAddress,
                via: "team-page",
                username: User.GetUserName());

            StatusMessage = "Submission done! Watch for the verdict in the list below.";
            return RedirectToAction(nameof(Team));
        }
    }
}
