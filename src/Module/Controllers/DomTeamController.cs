﻿using Ccs.Entities;
using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [Route("[area]/{cid:c(1)}/team")]
    [Authorize]
    [Authorize(Policy = "ContestHasTeam")]
    public class DomTeamController : ContestControllerBase<IDomContext>
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!Contest.IsTeamAccepted)
            {
                context.Result = IsWindowAjax
                    ? Message("401 Unauthorized",
                        "This contest is not active for you (yet).",
                        BootstrapColor.danger)
                    : View("AccessDenied");
            }

            ViewData["NavbarName"] = Ccs.CcsDefaults.NavbarTeam;
            ViewData["BigUrl"] = Url.Action("Home", "DomTeam");
            ViewData["ExtraMenu"] = "_NavButton";
            base.OnActionExecuting(context);
        }


        [HttpGet("[action]")]
        public Task<IActionResult> Scoreboard(
            [FromQuery(Name = "affiliations[]")] int[] affiliations,
            [FromQuery(Name = "categories[]")] int[] categories,
            [FromQuery(Name = "clear")] string clear = "")
            => CommonActions.DomScoreboard(this, !TooLate, false, clear == "clear", affiliations, categories);


        [HttpGet("[action]")]
        public IActionResult Print()
            => CommonActions.GetPrint(this);


        [HttpPost("[action]")]
        [AuditPoint(AuditlogType.Printing)]
        public Task<IActionResult> Print(AddPrintModel model)
            => CommonActions.PostPrint(this, model);


        [HttpGet]
        public async Task<IActionResult> Home()
        {
            var scb = await Context.GetScoreboardAsync();
            var bq = scb.Data.GetValueOrDefault(Contest.Team.TeamId);
            var cats = await Context.ListCategoriesAsync();
            var affs = await Context.ListAffiliationsAsync();
            var probs = await Context.ListProblemsAsync();

            int teamid = Contest.Team.TeamId;
            var clars = await Context.ListClarificationsAsync(
                c => (c.Sender == null && c.Recipient == null)
                || c.Recipient == teamid || c.Sender == teamid);

            var submits = await Context.ListSolutionsAsync(
                teamid: Contest.Team.TeamId,
                selector: (s, j) => new SubmissionViewModel
                {
                    Points = j.TotalScore ?? 0,
                    LanguageId = s.Language,
                    SubmissionId = s.Id,
                    Time = s.Time,
                    Skipped = s.Ignored,
                    Verdict = j.Status,
                    ProblemId = s.ProblemId,
                });

            return View(new TeamHomeViewModel
            {
                RankCache = bq.RankCache,
                ScoreCache = bq.ScoreCache,
                TeamId = bq.TeamId,
                TeamName = bq.TeamName,
                ContestId = Contest.Id,
                RankingStrategy = Contest.RankingStrategy,
                Problems = probs,
                Affiliation = affs[bq.AffiliationId],
                Category = cats[bq.CategoryId],
                Clarifications = clars,
                Submissions = submits,
            });
        }


        [HttpGet("problems")]
        public async Task<IActionResult> ProblemList()
        {
            var problems = await Context.ListProblemsAsync();
            return View(problems);
        }


        [HttpGet("problems/{prob}")]
        public async Task<IActionResult> ProblemView(string prob)
        {
            if (TooEarly && !Contest.IsJury) return NotFound();
            var problem = await Context.FindProblemAsync(prob, true);
            if (problem == null) return NotFound();

            var view = problem.Statement;
            if (string.IsNullOrEmpty(view)) return NotFound();

            ViewData["Content"] = view;
            return View();
        }


        [HttpGet("clarifications/add")]
        public IActionResult ClarificationAdd()
        {
            if (TooEarly) return Message("Clarification", "Contest has not started.");
            return Window(new AddClarificationModel());
        }


        [HttpGet("clarifications/{clarid}")]
        public async Task<IActionResult> ClarificationView(int clarid, bool needMore = true)
        {
            var toSee = await Context.FindClarificationAsync(clarid);
            var clars = Enumerable.Empty<Clarification>();

            if (toSee?.CheckPermission(Contest.Team.TeamId) ?? true)
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
            ViewData["TeamName"] = Contest.Team.TeamName;
            return Window(clars);
        }


        [HttpPost("clarifications/add")]
        [HttpPost("clarifications/{clarid}/reply")]
        [ValidateAntiForgeryToken]
        [AuditPoint(AuditlogType.Clarification)]
        public async Task<IActionResult> ClarificationReply(int? clarid, AddClarificationModel model)
        {
            var (cid, teamid) = (Contest.Id, Contest.Team.TeamId);

            Clarification replit = null;
            if (clarid.HasValue)
            {
                replit = await Context.FindClarificationAsync(clarid.Value);
                if (replit == null)
                {
                    ModelState.AddModelError("xys::replyto", "The clarification replied to is not found.");
                }
            }

            if (string.IsNullOrWhiteSpace(model.Body))
                ModelState.AddModelError("xys::empty", "No empty clarification");

            var probs = await Context.ListProblemsAsync();
            var usage = probs.ClarificationCategories.SingleOrDefault(cp => model.Type == cp.Item1);
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
        public async Task<IActionResult> Submit()
        {
            if (TooEarly && !Contest.IsJury)
            {
                return Message("Submit", "Contest not started.", BootstrapColor.danger);
            }

            return Window(new TeamCodeSubmitModel
            {
                Languages = await Context.ListLanguagesAsync(),
                Problems = await Context.ListProblemsAsync(),
            });
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(TeamCodeSubmitModel model)
        {
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
                via: "team-page",
                username: User.GetUserName());

            StatusMessage = "Submission done! Watch for the verdict in the list below.";
            return RedirectToAction(nameof(Home));
        }


        [HttpGet("[action]/{submitid}")]
        public async Task<IActionResult> Submission(int submitid)
        {
            int teamid = Contest.Team.TeamId;

            var model = await Context.FindSolutionAsync(
                submitid, (s, j) => new SubmissionViewModel
                {
                    SubmissionId = s.Id,
                    Points = j.TotalScore ?? 0,
                    LanguageId = s.Language,
                    Time = s.Time,
                    Skipped = s.Ignored,
                    Verdict = j.Status,
                    ProblemId = s.ProblemId,
                    CompilerOutput = j.CompileError,
                    SourceCode = s.SourceCode,
                });

            if (model == null) return NotFound();

            model.Problem = await Context.FindProblemAsync(model.ProblemId);
            model.Language = await Context.FindLanguageAsync(model.LanguageId);
            return Window(model);
        }
    }
}
