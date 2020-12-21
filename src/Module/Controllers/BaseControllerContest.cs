﻿using Ccs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    /// <summary>
    /// Base controller for contest related things.
    /// </summary>
    public class ContestControllerBase : ViewControllerBase
    {
        /// <summary>
        /// Context for contest controlling
        /// </summary>
        protected IContestContext Context { get; private set; }

        /// <summary>
        /// The contest entity
        /// </summary>
        protected Ccs.Entities.Contest Contest => Context.Contest;

        /// <summary>
        /// The messaging center
        /// </summary>
        protected IMediator Mediator { get; private set; }

        /// <summary>
        /// The team entity for current user
        /// </summary>
        protected Ccs.Entities.Team Team { get; private set; }

        /// <summary>
        /// Presents a view for printing codes.
        /// </summary>
        /// <returns>The action result.</returns>
        protected IActionResult Print()
        {
            if (!Contest.PrintingAvailable) return NotFound();
            return View("Print", new Models.AddPrintModel());
        }

        /// <summary>
        /// Resolve the printing request.
        /// </summary>
        /// <param name="model">The printing request.</param>
        /// <returns>The action result.</returns>
        protected async Task<IActionResult> Print(Models.AddPrintModel model)
        {
            if (!Contest.PrintingAvailable) return NotFound();

            using var stream = model.SourceFile.OpenReadStream();
            var bytes = new byte[model.SourceFile.Length];
            for (int i = 0; i < bytes.Length;)
                i += await stream.ReadAsync(bytes, i, bytes.Length - i);

            var Printings = Context.GetRequiredService<Ccs.Services.IPrintingStore>();
            var p = await Printings.CreateAsync(new Ccs.Entities.Printing
            {
                ContestId = Contest.Id,
                LanguageId = model.Language ?? "plain",
                FileName = System.IO.Path.GetFileName(model.SourceFile.FileName),
                Time = DateTimeOffset.Now,
                UserId = int.Parse(User.GetUserId()),
                SourceCode = bytes,
            });

            await HttpContext.AuditAsync(
                Entities.AuditlogType.Attachment,
                Contest.Id, User.GetUserName(),
                "added", $"{p.Id}",
                $"from {HttpContext.Connection.RemoteIpAddress}");

            StatusMessage = "File has been printed. Please wait.";
            return RedirectToAction("Home");
        }

        /// <summary>
        /// Presents a scoreboard view.
        /// </summary>
        /// <param name="isPublic">Whether accessing public board or restricted board.</param>
        /// <param name="isJury">Whether accessing jury board.</param>
        /// <param name="clear">Whether to clear the filter arguments.</param>
        /// <param name="filtered_affiliations">The filtered affiliation list.</param>
        /// <param name="filtered_categories">The filtered category list.</param>
        /// <returns>The task for creating action result.</returns>
        protected async Task<IActionResult> Scoreboard(
            bool isPublic, bool isJury, bool clear,
            int[] filtered_affiliations, int[] filtered_categories)
        {
            var scb = await Context.FetchScoreboardAsync();
            var affs = await Context.FetchAffiliationsAsync();
            var orgs = await Context.FetchCategoriesAsync();
            var probs = await Context.FetchProblemsAsync();

            if (!isJury)
                orgs = orgs.Values.Where(o => o.IsPublic).ToDictionary(c => c.Id);

            var board = new Ccs.Models.FullBoardViewModel
            {
                RankCache = scb.Data.Values,
                UpdateTime = scb.RefreshTime,
                Problems = probs,
                IsPublic = isPublic && !isJury,
                Categories = orgs,
                Contest = Contest,
                Affiliations = affs,
            };

            if (clear) filtered_categories = filtered_affiliations = Array.Empty<int>();

            if (filtered_affiliations.Length > 0)
            {
                var aff2 = filtered_affiliations.ToHashSet();
                board.RankCache = board.RankCache.Where(t => aff2.Contains(t.AffiliationId));
                ViewData["Filter_affiliations"] = aff2;
            }

            if (filtered_categories.Length > 0)
            {
                var cat2 = filtered_categories.ToHashSet();
                board.RankCache = board.RankCache.Where(t => cat2.Contains(t.CategoryId));
                ViewData["Filter_categories"] = cat2;
            }

            return View("Scoreboard", board);
        }

        /// <inheritdoc />
        [NonAction]
        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // check the contest info
            if (!context.RouteData.Values.TryGetValue("cid", out var __cid) ||
                !int.TryParse(__cid.ToString(), out int cid))
            {
                context.Result = NotFound();
                return;
            }

            // parse the base service
            Mediator = HttpContext.RequestServices.GetRequiredService<IMediator>();
            Context = await HttpContext.CreateContestContextAsync(cid);
            if (Context == null)
            {
                context.Result = NotFound();
                return;
            }
            else
            {
                HttpContext.Items[nameof(cid)] = cid;
                HttpContext.Features.Set(Context);
                ViewBag.Contest = Contest;
            }

            // the event of contest state change
            /*
            var stateNow = Contest.GetState();
            if (!Cache.TryGetValue($"`c{cid}`internal_state", out ContestState state))
            {
                Cache.Set($"`c{cid}`internal_state", stateNow, TimeSpan.FromDays(365));
                if (stateNow != ContestState.Finalized)
                    await Notifier.Update(cid, Contest, stateNow);
            }
            else if (state != stateNow)
            {
                Cache.Set($"`c{cid}`internal_state", stateNow, TimeSpan.FromDays(365));
                if (stateNow != ContestState.Finalized)
                    await Notifier.Update(cid, Contest, stateNow);
            }
            */

            // check the permission
            ViewData["IsJury"] = await Context.ValidateAsync(User);

            if (int.TryParse(User.GetUserId() ?? "-1", out int uid) && uid > 0)
            {
                ViewBag.Team = Team = await Context.FindTeamByUserAsync(uid);
                if (Team != null) ViewData["HasTeam"] = true;
            }

            if (!Contest.IsPublic &&
                !ViewData.ContainsKey("IsJury") &&
                !ViewData.ContainsKey("HasTeam"))
            {
                context.Result = NotFound();
                return;
            }

            await OnActionExecutingAsync(context);
            ViewData["ContestId"] = cid;

            if (context.Result == null)
                await OnActionExecutedAsync(await next());
        }

        /// <inheritdoc cref="Controller.OnActionExecuting(ActionExecutingContext)"/>
        /// <returns>A <see cref="Task"/> instance.</returns>
        [NonAction]
        public virtual Task OnActionExecutingAsync(ActionExecutingContext context)
        {
            OnActionExecuting(context);
            return Task.CompletedTask;
        }

        /// <inheritdoc cref="Controller.OnActionExecuted(ActionExecutedContext)"/>
        /// <returns>A <see cref="Task"/> instance.</returns>
        [NonAction]
        public virtual Task OnActionExecutedAsync(ActionExecutedContext context)
        {
            OnActionExecuted(context);
            return Task.CompletedTask;
        }
    }
}