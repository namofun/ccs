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
        public Task<IActionResult> Register([RPBinder("Form")] IRegisterProvider provider)
            => CommonActions.PostRegister(this, provider, nameof(Info));


        [HttpGet("[action]")]
        [Authorize]
        public Task<IActionResult> Register()
            => CommonActions.GetRegister(this, nameof(Info));


        [HttpGet("[action]")]
        public Task<IActionResult> Scoreboard(
            [FromQuery(Name = "affiliations[]")] int[] affiliations,
            [FromQuery(Name = "categories[]")] int[] categories,
            [FromQuery(Name = "clear")] string clear = "")
            => CommonActions.DomScoreboard(this, !TooLate, false, clear == "clear", affiliations, categories);
    }
}
