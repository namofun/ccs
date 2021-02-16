using Ccs;
using Ccs.Registration;
using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Route("[area]/{cid:c(1)}")]
    [Authorize(Policy = "ContestVisible")]
    [SupportStatusCodePage]
    public class DomPublicController : ContestControllerBase<IDomContext>
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewData["NavbarName"] = CcsDefaults.NavbarPublic;
            ViewData["BigUrl"] = Url.Action("Info", "DomPublic");
            ViewData["UseLightTheme"] = true;
            base.OnActionExecuting(context);
        }


        [HttpGet]
        public IActionResult Home()
        {
            if (Team == null) return RedirectToAction("Info", "DomPublic");
            return RedirectToAction("Home", "DomTeam");
        }


        [HttpGet("public")]
        public async Task<IActionResult> Info()
        {
            ViewBag.Affiliations = await Context.ListAffiliationsAsync();
            ViewBag.Categories = await Context.ListCategoriesAsync();
            ViewBag.Markdown = await Context.GetReadmeAsync();
            return View();
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        [AuditPoint(AuditlogType.Team)]
        [Authorize]
        public async Task<IActionResult> Register([RPBinder("Form")] IRegisterProvider provider)
        {
            if (Team != null)
            {
                StatusMessage = "Already registered";
                return RedirectToAction(nameof(Info));
            }

            var context = CreateRegisterProviderContext();
            if (provider == null
                || provider.JuryOrContestant
                || !await provider.IsAvailableAsync(context))
            {
                return NotFound();
            }

            var model = await provider.CreateInputModelAsync(context);
            await provider.ReadAsync(model, this);
            await provider.ValidateAsync(context, model, ModelState);

            if (ModelState.IsValid)
            {
                var output = await provider.ExecuteAsync(context, model);
                if (output is StatusMessageModel status)
                {
                    if (status.Succeeded)
                    {
                        await HttpContext.AuditAsync("created", status.TeamId?.ToString(), "via " + provider.Name);
                        StatusMessage = status.Content;
                        return RedirectToAction(nameof(Info));
                    }
                    else
                    {
                        StatusMessage = "Error " + status.Content;
                        return RedirectToAction(nameof(Register));
                    }
                }
            }

            StatusMessage = "Error " + ModelState.GetErrorStrings();
            return RedirectToAction(nameof(Register));
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Register()
        {
            if (Team != null)
            {
                StatusMessage = "Already registered";
                return RedirectToAction(nameof(Info));
            }

            var context = CreateRegisterProviderContext();
            ViewBag.Context = context;

            var items = new List<(IRegisterProvider, object)>();
            foreach (var (_, provider) in RPBinderAttribute.Get(HttpContext))
            {
                if (provider.JuryOrContestant) continue;
                if (!await provider.IsAvailableAsync(context)) continue;
                var input = await provider.CreateInputModelAsync(context);
                items.Add((provider, input));
            }

            if (items.Count == 0)
            {
                StatusMessage = "Registration is not for you.";
                return RedirectToAction(nameof(Info));
            }

            return View(items);
        }


        [HttpGet("[action]")]
        public Task<IActionResult> Scoreboard(
            [FromQuery(Name = "affiliations[]")] int[] affiliations,
            [FromQuery(Name = "categories[]")] int[] categories,
            [FromQuery(Name = "clear")] string clear = "")
            => CommonActions.DomScoreboard(this, !TooLate, false, clear == "clear", affiliations, categories);
    }
}
