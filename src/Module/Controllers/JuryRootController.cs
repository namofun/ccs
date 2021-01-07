﻿using Ccs.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule.Models;
using System;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Route("[area]/{cid}/[controller]")]
    public class JuryController : JuryControllerBase
    {
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
            => Scoreboard(false, true, clear == "clear", affiliations, categories);


        [HttpGet]
        public IActionResult Home()
            => View();


        [HttpGet("[action]")]
        [Authorize(Roles = "Administrator")]
        public IActionResult ResetEventFeed()
            => AskPost(
                title: "Reset event feed",
                message: "After reseting event feed, you can connect to CDS. " +
                    "But you shouldn't change any settings more, and you should use it only before contest start. " +
                    "Or it will lead to event missing. Are you sure?",
                area: "Contest", controller: "Jury", action: "ResetEventFeed",
                routeValues: new { cid = Contest.Id },
                type: BootstrapColor.warning);


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        [AuditPoint(Entities.AuditlogType.Contest)]
        [ActionName("ResetEventFeed")]
        public async Task<IActionResult> ResetEventFeedConfirmation()
        {
            await Mediator.Publish(new Ccs.Events.ScoreboardRefreshEvent(Contest, Problems));
            await HttpContext.AuditAsync("reset event", Contest.Id.ToString());
            StatusMessage = "Event feed reset.";
            return RedirectToAction(nameof(Home));
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
        [Authorize(Roles = "Administrator")]
        [AuditPoint(Entities.AuditlogType.Contest)]
        public async Task<IActionResult> ChangeState(string target)
        {
            var now = DateTimeOffset.Now;

            var newcont = new Contest
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

            await Context.UpdateContestAsync(
                _ => new Contest
                {
                    StartTime = newcont.StartTime,
                    EndTime = newcont.EndTime,
                    FreezeTime = newcont.FreezeTime,
                    UnfreezeTime = newcont.UnfreezeTime,
                });

            await HttpContext.AuditAsync("changed time", $"{Contest.Id}");
            return GoBackHome("Contest state changed.");
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Balloon()
        {
            if (!Contest.BalloonAvailable) return NotFound();
            var model = await Context.FetchBalloonsAsync();
            return View(model);
        }


        [HttpGet("balloon/{bid}/set-done")]
        public async Task<IActionResult> BalloonSetDone(int bid)
        {
            if (!Contest.BalloonAvailable) return NotFound();
            await Context.SetBalloonDoneAsync(bid);
            return RedirectToAction(nameof(Balloon));
        }


        [HttpGet("[action]")]
        [Authorize(Roles = "Administrator")]
        public IActionResult Assign()
            => Window(new JuryAssignModel());


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        [AuditPoint(Entities.AuditlogType.User)]
        public async Task<IActionResult> Assign(JuryAssignModel model)
        {
            var user = await UserManager.FindByNameAsync(model.UserName);

            if (user == null)
            {
                StatusMessage = "Error user not found.";
            }
            else
            {
                await Context.AssignJuryAsync(user);
                await HttpContext.AuditAsync("assigned jury", $"{user.Id}");
            }

            return RedirectToAction(nameof(Home));
        }


        [HttpGet("[action]/{uid}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Unassign(int uid)
        {
            var user = await UserManager.FindByIdAsync(uid);
            if (user == null) return NotFound();

            return AskPost(
                title: "Unassign jury",
                message: $"Do you want to unassign jury {user.UserName} (u{uid})?",
                area: "Contest", controller: "Jury", action: "Unassign",
                routeValues: new { uid, cid = Contest.Id },
                type: BootstrapColor.danger);
        }


        [HttpPost("[action]/{uid}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        [AuditPoint(Entities.AuditlogType.User)]
        [ActionName("Unassign")]
        public async Task<IActionResult> UnassignConfirmation(int uid)
        {
            var user = await UserManager.FindByIdAsync(uid);
            if (user == null) return NotFound();
            await Context.UnassignJuryAsync(user);
            StatusMessage = $"Jury role of user {user.UserName} unassigned.";
            await HttpContext.AuditAsync("unassigned jury", $"{uid}");
            return RedirectToAction(nameof(Home));
        }


        [HttpGet("[action]/{userName?}")]
        [Authorize(Roles = "Administrator")]
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
            ViewBag.Categories = await Context.FetchCategoriesAsync(false);
            return View(new JuryEditModel(Contest));
        }
        

        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        [AuditPoint(Entities.AuditlogType.Contest)]
        public async Task<IActionResult> Edit(JuryEditModel model)
        {
            // check the category id
            var cates = await Context.FetchCategoriesAsync(false);
            if (model.DefaultCategory != 0 && !cates.ContainsKey(model.DefaultCategory))
                ModelState.AddModelError("xys::nocat", "No corresponding category found.");

            // check time sequence
            if (!string.IsNullOrEmpty(model.StartTime) && string.IsNullOrEmpty(model.StopTime))
                ModelState.AddModelError("xys::startstop", "No stop time when start time filled.");

            bool contestTimeChanged = false;
            DateTimeOffset? startTime = null;
            TimeSpan? endTime = null, freezeTime = null, unfreezeTime = null;

            if (string.IsNullOrEmpty(model.StartTime))
                startTime = DateTimeOffset.Parse(model.StartTime);

            if (!string.IsNullOrWhiteSpace(model.StopTime))
                model.StopTime.TryParseAsTimeSpan(out endTime);

            if (!string.IsNullOrWhiteSpace(model.FreezeTime))
                model.FreezeTime.TryParseAsTimeSpan(out freezeTime);

            if (!string.IsNullOrWhiteSpace(model.UnfreezeTime))
                model.UnfreezeTime.TryParseAsTimeSpan(out unfreezeTime);

            int? defaultCat = null;
            if (model.DefaultCategory != 0)
                defaultCat = model.DefaultCategory;

            if (!endTime.HasValue
                || (freezeTime.HasValue && freezeTime.Value > endTime.Value)
                || (unfreezeTime.HasValue && unfreezeTime.Value < endTime.Value))
                ModelState.AddModelError("xys::time", "Time sequence is wrong.");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = cates;
                return View(model);
            }

            var cst = Contest;

            if (startTime != cst.StartTime
                || endTime != cst.EndTime
                || freezeTime != cst.FreezeTime
                || unfreezeTime != cst.UnfreezeTime)
                contestTimeChanged = true;

            await Context.UpdateContestAsync(
                _ => new Contest
                {
                    ShortName = model.ShortName,
                    Name = model.Name,
                    RankingStrategy = model.RankingStrategy,
                    IsPublic = model.IsPublic,
                    StartTime = startTime,
                    FreezeTime = freezeTime,
                    EndTime = endTime,
                    UnfreezeTime = unfreezeTime,
                    RegisterCategory = defaultCat,
                    BalloonAvailable = model.UseBalloon,
                    PrintingAvailable = model.UsePrintings,
                    StatusAvailable = model.StatusAvailable,
                });

            await HttpContext.AuditAsync("updated", $"{Contest.Id}", "via edit-page");

            StatusMessage = "Contest updated successfully.";
            if (contestTimeChanged)
                StatusMessage += " Scoreboard cache should be refreshed later.";

            return RedirectToAction(nameof(Home));
        }


        [HttpGet("[action]")]
        [Authorize(Roles = "Administrator")]
        public IActionResult RefreshCache()
            => AskPost(
                title: "Refresh scoreboard cache",
                message: "Do you want to refresh scoreboard cache? " +
                    "This will lead to a heavy database load in minutes.",
                area: "Contest", controller: "Jury", action: "RefreshCache",
                routeValues: new { cid = Contest.Id },
                type: BootstrapColor.warning);


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        [AuditPoint(Entities.AuditlogType.Contest)]
        [ActionName("RefreshCache")]
        public async Task<IActionResult> RefreshCacheConfirmation()
        {
            await Mediator.Publish(new Ccs.Events.ScoreboardRefreshEvent(Contest, Problems));
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
        [AuditPoint(Entities.AuditlogType.Contest)]
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