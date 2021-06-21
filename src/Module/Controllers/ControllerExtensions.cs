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
        public static IActionResult GetPrint<T>(
            this ContestControllerBase<T> that)
            where T : class, IContestContext
        {
            if (!that.Contest.Settings.PrintingAvailable) return that.NotFound();
            return that.View("Print", new Models.AddPrintModel());
        }


        public static async Task<IActionResult> PostPrint<T>(
            this ContestControllerBase<T> that,
            Models.AddPrintModel model)
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
            int[] filtered_affiliations, int[] filtered_categories, int? page)
            where T : class, IContestContext
        {
            if (that.Contest.Feature != Ccs.CcsDefaults.KindDom)
            {
                throw new NotSupportedException();
            }

            var pageVal = that.Contest.ShouldScoreboardPaging()
                ? page ?? 1
                : default(int?);

            that.ViewData["Paging"] = pageVal;
            if (pageVal.HasValue && pageVal < 1) return that.BadRequest();

            if (clear) filtered_categories = filtered_affiliations = Array.Empty<int>();
            var scb = await that.Context.GetScoreboardAsync();
            var board = new FullBoardViewModel(scb, isPublic && !isJury, isJury);

            if (filtered_affiliations.Length > 0)
            {
                var aff2 = filtered_affiliations.ToHashSet();
                board.FilteredAffiliations = aff2;
                that.ViewData["Filter_affiliations"] = aff2;
            }

            if (filtered_categories.Length > 0)
            {
                var cat2 = filtered_categories.ToHashSet();
                board.FilteredCategories = cat2;
                that.ViewData["Filter_categories"] = cat2;
            }

            if (that.Request.Cookies.TryGetValue("domjudge_teamselection", out var teamselection))
            {
                try
                {
                    var vals = teamselection.AsJson<string[]>();
                    if (vals != null && vals.Length <= 20 && vals.Length > 0)
                    {
                        var teams = new HashSet<int>();
                        for (int i = 0; i < vals.Length; i++)
                            if (int.TryParse(vals[i], out int teamid))
                                teams.Add(teamid);
                        board.FavoriteTeams = teams;
                    }
                }
                catch
                {
                    // The field `domjudge_teamselection` is wrongly set.
                    that.Response.Cookies.Delete("domjudge_teamselection");
                }
            }

            return that.View("Scoreboard", board);
        }


        public static async Task<IActionResult> GetRegister<T>(
            this ContestControllerBase<T> that,
            string homePage)
            where T : class, IContestContext
        {
            if (that.Contest.Team != null)
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


        public static async Task<IActionResult> PostRegister<T>(
            this ContestControllerBase<T> that,
            IRegisterProvider provider,
            string homePage,
            string registerPage = "Register")
            where T : class, IContestContext
        {
            if (that.Contest.Team != null)
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
                else
                {
                    throw new System.NotImplementedException();
                }
            }

            that.StatusMessage = "Error " + that.ModelState.GetErrorStrings();
            return that.RedirectToAction(registerPage);
        }


        public static IActionResult ClarificationAdd<T>(
            this ContestControllerBase<T> that)
            where T : class, ICompeteContext
        {
            if (that.TooEarly)
            {
                return that.Message("Clarification", "Contest has not started.");
            }
            else
            {
                return that.Window(new Models.AddClarificationModel());
            }
        }


        public static async Task<IActionResult> ClarificationView<T>(
            this ContestControllerBase<T> that,
            int clarid,
            bool needMore = true)
            where T : class, ICompeteContext
        {
            var toSee = await that.Context.FindClarificationAsync(clarid);
            var clars = Enumerable.Empty<Clarification>();

            if (toSee?.CheckPermission(that.Contest.Team.TeamId) ?? true)
            {
                clars = clars.Append(toSee);

                if (needMore && toSee.ResponseToId.HasValue)
                {
                    int respid = toSee.ResponseToId.Value;
                    var toSee2 = await that.Context.FindClarificationAsync(respid);
                    if (toSee2 != null) clars = clars.Prepend(toSee2);
                }
            }

            if (!clars.Any()) return that.NotFound();
            that.ViewData["TeamName"] = that.Contest.Team.TeamName;
            return that.Window(clars);
        }


        public static async Task<IActionResult> ClarificationReply<T>(
            this ContestControllerBase<T> that,
            int? clarid,
            Models.AddClarificationModel model,
            string HomePage)
            where T : class, ICompeteContext
        {
            var (cid, teamid) = (that.Contest.Id, that.Contest.Team.TeamId);

            Clarification replit = null;
            if (clarid.HasValue)
            {
                replit = await that.Context.FindClarificationAsync(clarid.Value);

                if (replit == null)
                {
                    that.ModelState.AddModelError(
                        "xys::replyto",
                        "The clarification replied to is not found.");
                }
            }

            if (string.IsNullOrWhiteSpace(model.Body))
            {
                that.ModelState.AddModelError(
                    "xys::empty",
                    "No empty clarification");
            }

            var probs = await that.Context.ListProblemsAsync();
            var usage = probs.ClarificationCategories.SingleOrDefault(cp => model.Type == cp.Item1);
            if (usage.Item1 == null)
            {
                that.ModelState.AddModelError(
                    "xys::error_cate",
                    "The category specified is wrong.");
            }

            if (!that.ModelState.IsValid)
            {
                that.StatusMessage = string.Join('\n',
                    that.ModelState.Values
                        .SelectMany(m => m.Errors)
                        .Select(e => e.ErrorMessage));
            }
            else
            {
                var clar = await that.Context.ClarifyAsync(
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

                await that.HttpContext.AuditAsync("added", $"{clar.Id}");
                that.StatusMessage = "Clarification sent to the jury.";
            }

            return that.RedirectToAction(HomePage);
        }
    }
}
