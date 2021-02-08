using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
            if ((Contest.StartTime ?? DateTimeOffset.MaxValue) >= DateTimeOffset.Now)
            {
                context.Result = NotStarted();
            }

            ViewData["NavbarName"] = Ccs.CcsDefaults.GymNavbar;
            ViewData["BigUrl"] = Url.Action("Home", "Gym");
            ViewData["UseLightTheme"] = true;
            await base.OnActionExecutingAsync(context);

            if (context.Result == null)
            {
                Statistics = await Context.StatisticsAsync(Team);
                ViewData["SubmissionStatistics"] = Statistics;
            }
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Standings(int page = 1)
        {
            if (page <= 0) return BadRequest();
            var scb = await Context.GetScoreboardAsync();
            var orgs = await Context.ListCategoriesAsync();
            var orgid = orgs.Values.Where(o => o.IsPublic).Select(a => a.Id).ToHashSet();

            return View(new GymStandingViewModel
            {
                OrganizationIds = orgid,
                CurrentPage = page,
                RankCache = scb.Data.Values,
                UpdateTime = scb.RefreshTime,
                Problems = await Context.ListProblemsAsync(),
                TeamMembers = await Context.GetTeamMembersAsync(),
                Statistics = await Context.StatisticsGlobalAsync(),
                ContestId = Contest.Id,
                RankingStrategy = Contest.RankingStrategy,
            });
        }


        [HttpGet]
        public async Task<IActionResult> Home()
        {
            return View(new GymHomeViewModel
            {
                Clarifications = await Context.ListClarificationsAsync(c => c.Recipient == null && c.Sender == null),
                Markdown = await Context.GetReadmeAsync(),
                MeStatistics = Statistics,
                AllStatistics = await Context.StatisticsGlobalAsync(),
                Problems = await Context.ListProblemsAsync(),
            });
        }


        [HttpGet("submissions/{sid}")]
        public async Task<IActionResult> Submission(int sid)
        {
            if (Team == null && Contest.Settings.StatusAvailable != 1)
            {
                return Forbid();
            }

            var model = await Context.FindSolutionAsync(
                sid, (s, j) => new SubmissionViewModel
                {
                    SubmissionId = s.Id,
                    Verdict = j.Status,
                    Time = s.Time,
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
            model.Language = await Context.FindLanguageAsync(model.LanguageId);

            if (model.Problem == null
                || model.Language == null
                || model.TeamId != Team?.TeamId
                && (Contest.Settings.StatusAvailable == 0
                || (Contest.Settings.StatusAvailable == 2 && Statistics.GetValueOrDefault(model.ProblemId).Accepted == 0)))
            {
                return Forbid();
            }

            var teamNames = await Context.ListTeamNamesAsync();
            model.TeamName = teamNames.GetValueOrDefault(model.TeamId, "");
            model.Runs = await Context.GetDetailsAsync(model.ProblemId, model.JudgingId);
            if (!model.Problem.Shared) model.Runs = model.Runs.Where(t => !t.Item2.IsSecret);
            return Window(model);
        }


        [HttpGet("problems/{prob}")]
        public async Task<IActionResult> ProblemView(string prob)
        {
            if (TooEarly && !ViewData.ContainsKey("IsJury")) return NotFound();
            var problem = await Context.FindProblemAsync(prob, true);
            if (problem == null) return NotFound();
            ViewBag.CurrentProblem = problem;

            var view = problem.Statement;
            if (string.IsNullOrEmpty(view)) return NotFound();
            ViewData["Content"] = view;

            var model = await Context.ListSolutionsAsync(problem.ProblemId, null, Team?.TeamId ?? -1000, true);
            return View(model);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Submit(string prob)
        {
            if (Team == null)
            {
                return Message("Submit", "You must have a team first.");
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


        //~
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


        //~
        [HttpPost("[action]")]
        [AuditPoint(AuditlogType.Team)]
        public async Task<IActionResult> Register(GymRegisterModel model)
        {
            if (ViewData.ContainsKey("HasTeam"))
                return GoBackHome("Already registered");
            if (Contest.Settings.RegisterCategory == null || User.IsInRole("Blocked"))
                return GoBackHome("Error registration closed.");

            string teamName;
            IUser[] uids;
            int affId;
            var uid = int.Parse(User.GetUserId());

            //if (model.AsIndividual)
            //{
                var affs = await Context.ListAffiliationsAsync(false);
                string defaultAff = User.IsInRole("Student") ? "jlu" : "null";
                var aff = affs.Values.FirstOrDefault(a => a.Abbreviation == defaultAff);
                if (aff == null)
                    return GoBackHome("No default affiliation.");
                affId = aff.Id;
                uids = null;// new[] { uid };

                var user = await UserManager.GetUserAsync(User);
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
                    return GoBackHome("Error team or team member.");
            //}

            var team = await Context.CreateTeamAsync(
                users: uids,
                team: new Team
                {
                    AffiliationId = affId,
                    ContestId = Contest.Id,
                    CategoryId = 0,
                    RegisterTime = DateTimeOffset.Now,
                    Status = 1,
                    TeamName = teamName,
                });

            await HttpContext.AuditAsync("added", $"{team.TeamId}");
*/
            StatusMessage = "Registration succeeded.";
            return RedirectToAction(nameof(Home));
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(TeamCodeSubmitModel model)
        {
            throw new Exception("Show re-display.");

            if (Team == null)
            {
                return GoBackHome("You should register first.", nameof(Register));
            }

            if (TooEarly && !Contest.IsJury)
            {
                return GoBackHome("Contest not started.");
            }

            var prob = await Context.FindProblemAsync(model.Problem);
            if (prob is null || !prob.AllowSubmit)
            {
                return GoBackHome("Error problem not found.");
            }

            var lang = await Context.FindLanguageAsync(model.Language);
            if (lang == null)
            {
                return GoBackHome("Error language not found.");
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


        [HttpGet("problems/{prob}/testcase/{tcid}/fetch/{filetype}")]
        public async Task<IActionResult> FetchTestcase(string prob, int tcid, string filetype)
        {
            if (filetype == "input") filetype = "in";
            else if (filetype == "output") filetype = "out";
            else return NotFound();

            if (TooEarly && !ViewData.ContainsKey("IsJury")) return NotFound();
            var problem = await Context.FindProblemAsync(prob);
            if (problem == null) return NotFound();

            var file = await Context.GetTestcaseAsync(problem, tcid, filetype);
            if (file == null || !file.Exists) return NotFound();

            return File(
                fileStream: file.CreateReadStream(),
                contentType: "application/octet-stream",
                fileDownloadName: $"{problem.ShortName}.t{tcid}.{filetype}");
        }


        [HttpGet("submissions")]
        public async Task<IActionResult> Submissions(int page = 1)
        {
            if (page <= 0) return BadRequest();
            var model = await Context.ListSolutionsAsync(page, 50);
            var tn = await Context.ListTeamNamesAsync();
            foreach (var solu in model)
            {
                solu.AuthorName = tn.GetValueOrDefault(solu.TeamId, string.Empty);
            }

            return View(model);
        }
    }
}
