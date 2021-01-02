using Ccs;
using Ccs.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using SatelliteSite.ContestModule.Models;
using SatelliteSite.IdentityModule.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Authorize]
    [Route("[controller]/{cid}")]
    public class GymController : ContestControllerBase
    {
        public bool TooEarly => Contest.GetState() < ContestState.Started;

        private IActionResult GoBackHome(string message)
        {
            StatusMessage = message;
            return RedirectToAction(nameof(Home));
        }

        private IActionResult NotStarted() => View("NotStarted");

        public override async Task OnActionExecutingAsync(ActionExecutingContext context)
        {
            if ((Contest.StartTime ?? DateTimeOffset.MaxValue) >= DateTimeOffset.Now)
                context.Result = NotStarted();
            
            await base.OnActionExecutingAsync(context);

            //if (context.Result == null && Team != null)
            //    ViewBag.TeamStatistics = await Facade.Teams
            //        .StatisticsSubmissionAsync(Team.ContestId, Team.TeamId);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Scoreboard(int cid)
        {
            ViewBag.Members = await Context.FetchTeamMembersAsync();
            return await Scoreboard(
                isPublic: Contest.GetState() < ContestState.Finalized,
                isJury: false, true, null, null);
        }


        [HttpGet]
        public async Task<IActionResult> Home()
        {
            //ViewBag.Statistics = await Facade.StatisticAcceptedAsync(cid);

            int? teamid = Team?.TeamId;
            ViewBag.Clarifications = await Context.ListClarificationsAsync(c => c.Recipient == null && c.Sender == null);
            ViewBag.Markdown = await Context.GetReadmeAsync();
            return View();
        }


        [HttpGet("[action]/{sid}")]
        public async Task<IActionResult> Submission(int sid)
        {
            if (Team == null) return Forbid();

            var model = await Context.FetchSolutionAsync(
                sid, (s, j) => new SubmissionViewModel
                {
                    SubmissionId = s.Id,
                    Verdict = j.Status,
                    Time = s.Time,
                    Problem = s.ProblemId,
                    CompilerOutput = j.CompileError,
                    Language = s.Language,
                    SourceCode = s.SourceCode,
                    Points = j.TotalScore ?? 0,
                    TeamId = s.TeamId,
                    JudgingId = j.Id,
                    ExecuteMemory = j.ExecuteMemory,
                    ExecuteTime = j.ExecuteTime,
                });

            if (model == null) return NotFound();
            Dictionary<int, (int ac, int tot)> substat = ViewBag.TeamStatistics;
            model.TeamName = (await Context.FindTeamByIdAsync(model.TeamId)).TeamName;

            if (model.TeamId != Team.TeamId)
            {
                if (Contest.StatusAvailable == 2)
                {
                    if (substat.GetValueOrDefault(model.Problem).ac == 0)
                        return Forbid();
                }
                else if (Contest.StatusAvailable == 0)
                {
                    return Forbid();
                }
            }

            //var tcs = await judgings.GetDetailsAsync(model.Problem.ProblemId, model.JudgingId);
            //if (!model.Problem.Shared)
            //    tcs = tcs.Where(t => !t.Item2.IsSecret);
            //ViewBag.Runs = tcs;

            return Window(model);
        }


        [HttpGet("problem/{prob}")]
        public async Task<IActionResult> ProblemView(string prob)
        {
            if (TooEarly && !ViewData.ContainsKey("IsJury")) return NotFound();
            var problem = Problems.Find(prob);
            if (problem == null) return NotFound();
            ViewBag.CurrentProblem = problem;

            var view = problem.Statement;
            if (string.IsNullOrEmpty(view)) return NotFound();
            ViewData["Content"] = view;

            var model = await Context.FetchSolutionsAsync(problem.ProblemId, null, Team?.TeamId ?? -1000, true);
            return View(model);
        }


        [HttpGet("[action]")]
        public IActionResult Submit(string prob)
        {
            if (TooEarly && !ViewData.ContainsKey("IsJury"))
                return Message("Submit", "Contest not started.", BootstrapColor.danger);
            return Window(new TeamCodeSubmitModel { Problem = prob });
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Register()
        {
            if (Team != null) return NotFound();
            /*var items = await training.ListAsync(int.Parse(User.GetUserId()), true);
            ViewData["TeamsJson"] = items.Select(g => new
            {
                team = new { name = g.Key.TeamName, id = g.Key.TrainingTeamId },
                users = g.Select(v => new { name = v.UserName, id = v.UserId }).ToList()
            })
            .ToJson();*/
            await Task.CompletedTask;

            return View(new GymRegisterModel { AsIndividual = true });
        }


        [HttpPost("[action]")]
        [AuditPoint(Entities.AuditlogType.Team)]
        public async Task<IActionResult> Register(GymRegisterModel model)
        {
            if (ViewData.ContainsKey("HasTeam"))
                return GoBackHome("Already registered");
            if (Contest.RegisterCategory == null || User.IsInRole("Blocked"))
                return GoBackHome("Error registration closed.");

            string teamName;
            IUser[] uids;
            int affId;
            var uid = int.Parse(User.GetUserId());

            //if (model.AsIndividual)
            //{
                var affs = await Context.FetchAffiliationsAsync(false);
                string defaultAff = User.IsInRole("Student") ? "jlu" : "null";
                var aff = affs.Values.FirstOrDefault(a => a.Abbreviation == defaultAff);
                if (aff == null)
                    return GoBackHome("No default affiliation.");
                affId = aff.Id;
                uids = null;// new[] { uid };

                var user = await HttpContext.RequestServices.GetRequiredService<IUserManager>().GetUserAsync(User);
                //if (user.StudentId.HasValue && user.StudentVerified)
                //    teamName = (await userManager.FindStudentAsync(user.StudentId.Value)).Name;
                //else
                    teamName = user.NickName;
                teamName ??= user.UserName;
            /*}
            else
            {
                
                var team = await training.FindTeamByIdAsync(model.TeamId);
                if (team == null)
                    return GoBackHome("Error team or team member.");
                (teamName, affId) = (team.TeamName, team.AffiliationId);

                var users = await training.ListMembersAsync(team, true);
                uids = (model.UserIds ?? Enumerable.Empty<int>()).Append(uid).Distinct().ToArray();
                if (uids.Except(users.Select(g => g.UserId)).Any())
                    return GoBackHome("Error team or team member.");*/
            //}

            var team = await Context.CreateTeamAsync(
                users: uids,
                team: new Team
                {
                    AffiliationId = affId,
                    ContestId = Contest.Id,
                    CategoryId = Contest.RegisterCategory.Value,
                    RegisterTime = DateTimeOffset.Now,
                    Status = 1,
                    TeamName = teamName,
                });

            await HttpContext.AuditAsync("added", $"{team.TeamId}");
            StatusMessage = "Registration succeeded.";
            return RedirectToAction(nameof(Home));
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(TeamCodeSubmitModel model)
        {
            if (!ViewData.ContainsKey("HasTeam"))
            {
                StatusMessage = "You should register first.";
                return RedirectToAction(nameof(Register));
            }

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
                via: "gym-page",
                username: User.GetUserName());

            StatusMessage = "Submission done!";
            return RedirectToAction(nameof(ProblemView), new { prob = prob.ShortName });
        }


        [HttpGet("problem/{prob}/testcase/{tcid}/fetch/{filetype}")]
        public async Task<IActionResult> FetchTestcase(string prob, int tcid, string filetype)
        {
            if (filetype == "input") filetype = "in";
            else if (filetype == "output") filetype = "out";
            else return NotFound();

            if (TooEarly && !ViewData.ContainsKey("IsJury")) return NotFound();
            var problem = Problems.Find(prob);
            if (problem == null) return NotFound();

            //var tc = await testcases.FindAsync(problem.ProblemId, tcid);
            //if (tc == null) return NotFound();
            //if (tc.IsSecret && !problem.Shared) return NotFound();

            //var file = testcases.GetFile(tc, filetype);
            //if (!file.Exists) return NotFound();

            return File(
                fileStream: null, // file.CreateReadStream(),
                contentType: "application/octet-stream",
                fileDownloadName: $"{problem.ShortName}.t{tcid}.{filetype}");
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Submissions(int page = 1)
        {
            if (page <= 0) return BadRequest();
            var model = await Context.FetchSolutionsAsync(page, 50);
            ViewBag.TeamsName = await Context.FetchTeamNamesAsync();
            return View(model);
        }
    }
}
