using Ccs;
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
    [Route("[area]/{cid}/[controller]")]
    public partial class TeamController : ContestControllerBase
    {
        public bool TooEarly => Contest.GetState() < ContestState.Started;

        private new IActionResult NotFound() => StatusCodePage(404);

        public override async Task OnActionExecutingAsync(ActionExecutingContext context)
        {
            if (Contest.Gym)
                context.Result = RedirectToAction("Home", "Gym");

            else if (Team == null || Team.Status != 1)
            {
                context.Result = IsWindowAjax
                    ? Message("401 Unauthorized",
                        "This contest is not active for you (yet).",
                        BootstrapColor.danger)
                    : View("AccessDenied");
            }

            await base.OnActionExecutingAsync(context);
        }


        [HttpGet("[action]")]
        public new IActionResult Print()
            => base.Print();


        [HttpPost("[action]")]
        [AuditPoint(Entities.AuditlogType.Attachment)]
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


        [HttpGet]
        public async Task<IActionResult> Home()
        {
            int teamid = Team.TeamId;
            var board = await Context.SingleBoardAsync(teamid);
            
            ViewBag.Clarifications = await Context.ListClarificationsAsync(
                c => (c.Sender == null && c.Recipient == null)
                || c.Recipient == teamid || c.Sender == teamid);

            ViewBag.Submissions = 
                await submits.ListWithJudgingAsync(
                predicate: s => s.ContestId == cid && s.Author == teamid,
                selector: (s, j) => new SubmissionViewModel
                {
                    Grade = j.TotalScore ?? 0,
                    Language = Languages[s.Language],
                    SubmissionId = s.SubmissionId,
                    Time = s.Time,
                    Verdict = j.Status,
                    Problem = Problems.Find(s.ProblemId),
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

            var models = await submissions.ListWithJudgingAsync(
                predicate: s => s.ContestId == cid && s.SubmissionId == sid,
                selector: (s, j) => new SubmissionViewModel
                {
                    SubmissionId = s.SubmissionId,
                    Grade = j.TotalScore ?? 0,
                    Language = Languages[s.Language],
                    Time = s.Time,
                    Verdict = j.Status,
                    Problem = Problems.Find(s.ProblemId),
                    CompilerOutput = j.CompileError,
                    SourceCode = s.SourceCode,
                });

            var model = models.SingleOrDefault();
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
        [AuditPoint(Entities.AuditlogType.Clarification)]
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

            var usage = Problems.GetClarificationCategories().SingleOrDefault(cp => model.Type == cp.Item1);
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

            return RedirectToAction(nameof(Home));
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
                return RedirectToAction(nameof(Home));
            }

            var prob = Problems.Find(model.Problem);
            if (prob is null || !prob.AllowSubmit)
            {
                StatusMessage = "Error problem not found.";
                return RedirectToAction(nameof(Home));
            }

            var langs = await Context.FetchLanguagesAsync();
            var lang = langs.FirstOrDefault(l => l.Id == model.Language);
            if (lang == null)
            {
                StatusMessage = "Error language not found.";
                return RedirectToAction(nameof(Home));
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
            return RedirectToAction(nameof(Home));
        }
    }
}
