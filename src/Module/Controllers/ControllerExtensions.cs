using Ccs.Entities;
using Ccs.Models;
using Ccs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    /// <summary>
    /// Provides several basic functions.
    /// </summary>
    internal static class CommonActions
    {
        /// <summary>
        /// Presents a view for printing codes.
        /// </summary>
        /// <param name="that">The controller.</param>
        /// <returns>The action result.</returns>
        public static IActionResult GetPrint<T>(ContestControllerBase<T> that)
            where T : class, IContestContext
        {
            if (!that.Contest.Settings.PrintingAvailable) return that.NotFound();
            return that.View("Print", new Models.AddPrintModel());
        }

        /// <summary>
        /// Resolve the printing request.
        /// </summary>
        /// <param name="that">The controller.</param>
        /// <param name="model">The printing request.</param>
        /// <returns>The action result.</returns>
        public static async Task<IActionResult> PostPrint<T>(this ContestControllerBase<T> that, Models.AddPrintModel model)
            where T : class, IContestContext
        {
            if (!that.Contest.Settings.PrintingAvailable) return that.NotFound();

            using var stream = model.SourceFile.OpenReadStream();
            var bytes = new byte[model.SourceFile.Length];
            for (int i = 0; i < bytes.Length;)
                i += await stream.ReadAsync(bytes, i, bytes.Length - i);

            var Printings = that.HttpContext.RequestServices.GetRequiredService<IPrintingService>();
            var p = await Printings.CreateAsync(new Printing
            {
                ContestId = that.Contest.Id,
                LanguageId = model.Language ?? "plain",
                FileName = System.IO.Path.GetFileName(model.SourceFile.FileName),
                Time = DateTimeOffset.Now,
                UserId = int.Parse(that.User.GetUserId()),
                SourceCode = bytes,
            });

            await that.HttpContext.AuditAsync(
                AuditlogType.Printing,
                that.Contest.Id, that.User.GetUserName(),
                "added", $"{p.Id}",
                $"from {that.HttpContext.Connection.RemoteIpAddress}");

            that.StatusMessage = "File has been printed. Please wait.";
            return that.RedirectToAction("Home");
        }

        /// <summary>
        /// Presents a scoreboard view.
        /// </summary>
        /// <param name="that">The controller.</param>
        /// <param name="isPublic">Whether accessing public board or restricted board.</param>
        /// <param name="isJury">Whether accessing jury board.</param>
        /// <param name="clear">Whether to clear the filter arguments.</param>
        /// <param name="filtered_affiliations">The filtered affiliation list.</param>
        /// <param name="filtered_categories">The filtered category list.</param>
        /// <returns>The task for creating action result.</returns>
        public static async Task<IActionResult> DomScoreboard<T>(
            this ContestControllerBase<T> that,
            bool isPublic, bool isJury, bool clear,
            int[] filtered_affiliations, int[] filtered_categories)
            where T : class, IContestContext
        {
            if (that.Contest.Kind == 2)
            {
                throw new NotSupportedException();
            }

            var probs = await that.Context.ListProblemsAsync();
            var scb = await that.Context.GetScoreboardAsync();
            var affs = await that.Context.ListAffiliationsAsync();
            var orgs = await that.Context.ListCategoriesAsync();

            if (!isJury)
            {
                orgs = orgs.Values.Where(o => o.IsPublic).ToDictionary(c => c.Id);
            }

            var board = new FullBoardViewModel
            {
                RankCache = scb.Data.Values,
                UpdateTime = scb.RefreshTime,
                Problems = probs,
                IsPublic = isPublic && !isJury,
                Categories = orgs,
                ContestId = that.Contest.Id,
                RankingStrategy = that.Contest.RankingStrategy,
                Affiliations = affs,
            };

            if (clear) filtered_categories = filtered_affiliations = Array.Empty<int>();

            if (filtered_affiliations.Length > 0)
            {
                var aff2 = filtered_affiliations.ToHashSet();
                board.RankCache = board.RankCache.Where(t => aff2.Contains(t.AffiliationId));
                that.ViewData["Filter_affiliations"] = aff2;
            }

            if (filtered_categories.Length > 0)
            {
                var cat2 = filtered_categories.ToHashSet();
                board.RankCache = board.RankCache.Where(t => cat2.Contains(t.CategoryId));
                that.ViewData["Filter_categories"] = cat2;
            }

            return that.View("Scoreboard", board);
        }
    }
}
