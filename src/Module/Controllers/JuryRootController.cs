using Ccs;
using Ccs.Entities;
using Ccs.Models;
using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Authorize(Policy = "ContestIsJury")]
    [Route("[area]/{cid:c(7)}/[controller]")]
    public class JuryController : JuryControllerBase<IJuryContext>
    {
        [HttpGet("/[area]/{cid:c(1)}/[controller]/[action]")]
        public IActionResult Print()
            => CommonActions.GetPrint(this);


        [HttpPost("/[area]/{cid:c(1)}/[controller]/[action]")]
        [ValidateAntiForgeryToken]
        [AuditPoint(AuditlogType.Printing)]
        public Task<IActionResult> Print(AddPrintModel model)
            => CommonActions.PostPrint(this, model);


        [HttpGet("/[area]/{cid:c(1)}/[controller]/[action]")]
        public Task<IActionResult> Scoreboard(
            [FromQuery(Name = "affiliations[]")] int[] affiliations,
            [FromQuery(Name = "categories[]")] int[] categories,
            [FromQuery(Name = "clear")] string clear = "")
            => CommonActions.DomScoreboard(this, false, true, clear == "clear", affiliations, categories);


        [HttpGet]
        public IActionResult Home()
            => View();


        [HttpGet("/[area]/{cid:c(1)}/[controller]/[action]")]
        [Authorize(Policy = "ContestIsAdministrator")]
        public IActionResult ResetEventFeed()
        {
            if (!Contest.Settings.EventAvailable) return NotFound();
            return View();
        }


        [HttpPost("/[area]/{cid:c(1)}/[controller]/[action]")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ContestIsAdministrator")]
        [AuditPoint(AuditlogType.Contest)]
        [ActionName("ResetEventFeed")]
        public async Task<IActionResult> ResetEventFeedConfirmation()
        {
            if (!Contest.Settings.EventAvailable || !InAjax) return BadRequest();
            await HttpContext.AuditAsync("reset event requested", Contest.Id.ToString());
            return new ResetEventResult();
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Auditlog(int page = 1)
        {
            if (page <= 0) return NotFound();
            var model = await Context.ViewLogsAsync(page, 1000);
            return View(model);
        }


        [HttpPost("[action]/{target}")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ContestIsAdministrator")]
        [AuditPoint(AuditlogType.Contest)]
        public async Task<IActionResult> ChangeState(string target)
        {
            var now = DateTimeOffset.Now;

            var newcont = new TimeOnlyModel
            {
                StartTime = Contest.StartTime,
                EndTime = Contest.EndTime,
                FreezeTime = Contest.FreezeTime,
                UnfreezeTime = Contest.UnfreezeTime,
            };

            var state = newcont.GetState(now);

            if (target == "startnow")
            {
                if (!newcont.EndTime.HasValue)
                    return GoBackHome("Error no end time specified.");
                now += TimeSpan.FromSeconds(30);

                if (newcont.StartTime.HasValue && newcont.StartTime.Value < now)
                    return GoBackHome("Error starting contest for the remaining time is less than 30 secs.");
                newcont.StartTime = now;
            }
            else if (target == "freeze")
            {
                if (state != ContestState.Started)
                    return GoBackHome("Error contest is not started.");
                newcont.FreezeTime = now - newcont.StartTime.Value;
            }
            else if (target == "endnow")
            {
                if (state != ContestState.Started && state != ContestState.Frozen)
                    return GoBackHome("Error contest has not started or has ended.");
                newcont.EndTime = now - newcont.StartTime.Value;

                if (newcont.FreezeTime.HasValue && newcont.FreezeTime.Value > newcont.EndTime.Value)
                    newcont.FreezeTime = newcont.EndTime;
            }
            else if (target == "unfreeze")
            {
                if (state != ContestState.Ended)
                    return GoBackHome("Error contest has not ended.");
                newcont.UnfreezeTime = now - newcont.StartTime.Value;
            }
            else if (target == "delay")
            {
                if (state != ContestState.ScheduledToStart)
                    return GoBackHome("Error contest has been started.");
                newcont.StartTime = null;
            }

            var sta = newcont.StartTime;
            var end = newcont.EndTime?.TotalSeconds;
            var frz = newcont.FreezeTime?.TotalSeconds;
            var unf = newcont.UnfreezeTime?.TotalSeconds;
            await Context.UpdateContestAsync(
                _ => new Contest
                {
                    StartTime = sta,
                    EndTimeSeconds = end,
                    FreezeTimeSeconds = frz,
                    UnfreezeTimeSeconds = unf,
                });

            await HttpContext.AuditAsync("changed time", $"{Contest.Id}");
            return GoBackHome("Contest state changed.");
        }



        [HttpGet("[action]")]
        [Authorize(Policy = "ContestIsAdministrator")]
        public IActionResult Assign()
            => Window(new JuryAssignModel { Level = JuryLevel.Jury });


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ContestIsAdministrator")]
        [AuditPoint(AuditlogType.User)]
        public async Task<IActionResult> Assign(JuryAssignModel model)
        {
            var user = await UserManager.FindByNameAsync(model.UserName);

            if (user == null)
            {
                StatusMessage = "Error user not found.";
            }
            else
            {
                await Context.AssignJuryAsync(user, model.Level);
                await HttpContext.AuditAsync("assigned jury", $"{user.Id}");
            }

            return RedirectToAction(nameof(Home));
        }


        [HttpGet("[action]/{userid}")]
        [Authorize(Policy = "ContestIsAdministrator")]
        public async Task<IActionResult> Unassign(int userid)
        {
            var user = await UserManager.FindByIdAsync(userid);
            if (user == null) return NotFound();

            return AskPost(
                title: "Unassign jury",
                message: $"Do you want to unassign jury {user.UserName} (u{userid})?",
                area: "Contest", controller: "Jury", action: "Unassign",
                routeValues: new { userid, cid = Contest.Id },
                type: BootstrapColor.danger);
        }


        [HttpPost("[action]/{userid}")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ContestIsAdministrator")]
        [AuditPoint(AuditlogType.User)]
        [ActionName("Unassign")]
        public async Task<IActionResult> UnassignConfirmation(int userid)
        {
            var user = await UserManager.FindByIdAsync(userid);
            if (user == null) return NotFound();
            await Context.UnassignJuryAsync(user);
            StatusMessage = $"Jury role of user {user.UserName} unassigned.";
            await HttpContext.AuditAsync("unassigned jury", $"{userid}");
            return RedirectToAction(nameof(Home));
        }


        [HttpGet("[action]/{userName?}")]
        [Authorize(Policy = "ContestIsAdministrator")]
        public async Task<IActionResult> TestUser(string userName)
        {
            if (userName != null)
            {
                var user = await UserManager.FindByNameAsync(userName);
                if (user == null)
                    return Content("No such user.", "text/html");
                return Content("", "text/html");
            }
            else
            {
                return Content("Please enter the username.", "text/html");
            }
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Edit()
        {
            ViewBag.Categories = await Context.ListCategoriesAsync(false);
            ViewBag.Languages = await Context.ListLanguagesAsync(false);
            return View(new JuryEditModel(Contest));
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        [AuditPoint(AuditlogType.Contest)]
        public async Task<IActionResult> Edit(JuryEditModel model)
        {
            // check the category id
            var cates = await Context.ListCategoriesAsync(false);
            if ((model.RegisterCategory?.Values ?? Enumerable.Empty<int>()).Except(cates.Keys.Append(0)).Any())
                ModelState.AddModelError(nameof(model.RegisterCategory), "No corresponding category found.");

            // check time sequence
            if (Contest.Kind != CcsDefaults.KindProblemset
                && !string.IsNullOrEmpty(model.StartTime)
                && string.IsNullOrEmpty(model.StopTime))
                ModelState.AddModelError(nameof(model.StopTime), "No stop time when start time filled.");

            bool contestTimeChanged = false;
            DateTimeOffset? startTime = null;
            TimeSpan? endTime = null, freezeTime = null, unfreezeTime = null;

            if (!string.IsNullOrEmpty(model.StartTime))
                startTime = DateTimeOffset.Parse(model.StartTime);

            if (!string.IsNullOrWhiteSpace(model.StopTime))
                model.StopTime.TryParseAsTimeSpan(out endTime);

            if (!string.IsNullOrWhiteSpace(model.FreezeTime))
                model.FreezeTime.TryParseAsTimeSpan(out freezeTime);

            if (!string.IsNullOrWhiteSpace(model.UnfreezeTime))
                model.UnfreezeTime.TryParseAsTimeSpan(out unfreezeTime);

            var defaultCat = model.RegisterCategory?
                .Where(k => k.Value != 0)
                .ToDictionary(k => k.Key, v => v.Value);
            if (defaultCat?.Count == 0) defaultCat = null;

            if (!endTime.HasValue
                || (freezeTime.HasValue && freezeTime.Value > endTime.Value)
                || (unfreezeTime.HasValue && unfreezeTime.Value < endTime.Value))
                ModelState.AddModelError("xys::time", "Time sequence is wrong.");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = cates;
                ViewBag.Languages = await Context.ListLanguagesAsync(false);
                return View(model);
            }

            var freezeTimeSeconds = freezeTime?.TotalSeconds;
            var endTimeSeconds = endTime?.TotalSeconds;
            var unfreezeTimeSeconds = unfreezeTime?.TotalSeconds;

            if (startTime != Contest.StartTime
                || endTime != Contest.EndTime
                || freezeTime != Contest.FreezeTime
                || unfreezeTime != Contest.UnfreezeTime)
                contestTimeChanged = true;

            var settings = Contest.Settings.Clone();
            settings.BalloonAvailable = model.UseBalloon;
            settings.EventAvailable = model.UseEvents;
            settings.Languages = model.Languages;
            settings.PrintingAvailable = model.UsePrintings;
            settings.RegisterCategory = defaultCat;
            settings.StatusAvailable = model.StatusAvailable;
            var settingsJson = settings.ToString();

            await Context.UpdateContestAsync(
                _ => new Contest
                {
                    ShortName = model.ShortName,
                    Name = model.Name,
                    RankingStrategy = model.RankingStrategy,
                    IsPublic = model.IsPublic,
                    StartTime = startTime,
                    FreezeTimeSeconds = freezeTimeSeconds,
                    EndTimeSeconds = endTimeSeconds,
                    UnfreezeTimeSeconds = unfreezeTimeSeconds,
                    SettingsJson = settingsJson,
                });

            await HttpContext.AuditAsync("updated", $"{Contest.Id}", "via edit-page");

            StatusMessage = "Contest updated successfully.";
            if (contestTimeChanged)
                StatusMessage += " Scoreboard cache should be refreshed later.";

            return RedirectToAction(nameof(Home));
        }


        [HttpGet("[action]")]
        [Authorize(Policy = "ContestIsAdministrator")]
        public IActionResult RefreshCache()
            => View();


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ContestIsAdministrator")]
        [AuditPoint(AuditlogType.Contest)]
        [ActionName("RefreshCache")]
        public async Task<IActionResult> RefreshCacheConfirmation()
        {
            await Mediator.Publish(new Ccs.Events.ScoreboardRefreshEvent(Context));
            StatusMessage = "Scoreboard cache has been refreshed.";
            await HttpContext.AuditAsync("refresh scoreboard cache", Contest.Id.ToString());
            return RedirectToAction(nameof(Home));
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Description()
        {
            var content = await Context.GetReadmeAsync(true);
            return View(new JuryMarkdownModel { Markdown = content });
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        [AuditPoint(AuditlogType.Contest)]
        public async Task<IActionResult> Description(JuryMarkdownModel model)
        {
            model.Markdown ??= "";
            await Context.SetReadmeAsync(model.Markdown);
            await HttpContext.AuditAsync("updated", $"{Contest.Id}", "description");
            return View(model);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Updates()
            => Json(await Context.GetUpdatesAsync());
    }
}
