using Ccs;
using Ccs.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Route("[area]/{cid:c(1)}")]
    [Authorize(Policy = "ContestVisible")]
    [SupportStatusCodePage]
    public class DomPublicController : ContestControllerBase
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewData["NavbarName"] = CcsDefaults.PublicNavbar;
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
            ViewBag.Affiliations = await Context.FetchAffiliationsAsync();
            ViewBag.Categories = await Context.FetchCategoriesAsync();
            ViewBag.Markdown = await Context.GetReadmeAsync();
            return View();
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        [AuditPoint(AuditlogType.Team)]
        [Authorize]
        public async Task<IActionResult> Register()
        {
            if (ViewData.ContainsKey("HasTeam"))
            {
                StatusMessage = "Already registered";
                return RedirectToAction(nameof(Info));
            }

            await Task.CompletedTask;
            return NoContent();
        }


        [HttpGet("[action]")]
        public Task<IActionResult> Scoreboard(
            [FromQuery(Name = "affiliations[]")] int[] affiliations,
            [FromQuery(Name = "categories[]")] int[] categories,
            [FromQuery(Name = "clear")] string clear = "")
            => Scoreboard(isPublic: !TooLate, isJury: false, clear == "clear", affiliations, categories);
    }
}
