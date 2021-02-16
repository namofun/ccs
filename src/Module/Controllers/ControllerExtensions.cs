using Ccs.Entities;
using Ccs.Models;
using Ccs.Registration;
using Ccs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    /// <summary>
    /// Provides several basic functions.
    /// </summary>
    internal static class CommonActions
    {
        public static IActionResult GetPrint<T>(this ContestControllerBase<T> that)
            where T : class, IContestContext
        {
            if (!that.Contest.Settings.PrintingAvailable) return that.NotFound();
            return that.View("Print", new Models.AddPrintModel());
        }


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


        public static async Task<IActionResult> DomScoreboard<T>(
            this ContestControllerBase<T> that,
            bool isPublic, bool isJury, bool clear,
            int[] filtered_affiliations, int[] filtered_categories)
            where T : class, IContestContext
        {
            if (that.Contest.Kind != Ccs.CcsDefaults.KindDom)
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


        public static async Task<IActionResult> GetRegister<T>(this ContestControllerBase<T> that, string homePage)
            where T : class, IContestContext
        {
            if (that.Team != null)
            {
                that.StatusMessage = "Already registered";
                return that.RedirectToAction(homePage);
            }

            var context = that.CreateRegisterProviderContext();
            that.ViewBag.Context = context;

            var items = new List<(IRegisterProvider, object)>();
            foreach (var (_, provider) in RPBinderAttribute.Get(that.HttpContext))
            {
                if (provider.JuryOrContestant) continue;
                if (!await provider.IsAvailableAsync(context)) continue;
                var input = await provider.CreateInputModelAsync(context);
                items.Add((provider, input));
            }

            if (items.Count == 0)
            {
                that.StatusMessage = "Registration is not for you.";
                return that.RedirectToAction(homePage);
            }

            return that.View("Register", items);
        }


        public static async Task<IActionResult> PostRegister<T>(this ContestControllerBase<T> that, IRegisterProvider provider, string homePage, string registerPage = "Register")
            where T : class, IContestContext
        {
            if (that.Team != null)
            {
                that.StatusMessage = "Already registered";
                return that.RedirectToAction(homePage);
            }

            var context = that.CreateRegisterProviderContext();
            if (provider == null
                || provider.JuryOrContestant
                || !await provider.IsAvailableAsync(context))
            {
                return that.NotFound();
            }

            var model = await provider.CreateInputModelAsync(context);
            await provider.ReadAsync(model, that);
            await provider.ValidateAsync(context, model, that.ModelState);

            if (that.ModelState.IsValid)
            {
                var output = await provider.ExecuteAsync(context, model);
                if (output is StatusMessageModel status)
                {
                    if (status.Succeeded)
                    {
                        await that.HttpContext.AuditAsync("created", status.TeamId?.ToString(), "via " + provider.Name);
                        that.StatusMessage = status.Content;
                        return that.RedirectToAction(homePage);
                    }
                    else
                    {
                        that.StatusMessage = "Error " + status.Content;
                        return that.RedirectToAction(registerPage);
                    }
                }
            }

            that.StatusMessage = "Error " + that.ModelState.GetErrorStrings();
            return that.RedirectToAction(registerPage);
        }
    }
}
