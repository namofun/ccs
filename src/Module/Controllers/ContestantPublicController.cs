using Ccs.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SatelliteSite.IdentityModule.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Route("[area]/{cid}/[action]")]
    public class PublicController : ContestControllerBase
    {
        public override Task OnActionExecutingAsync(ActionExecutingContext context)
        {
            if (Contest.Gym)
                context.Result = RedirectToAction("Home", "Gym");
            return base.OnActionExecutingAsync(context);
        }


        [HttpGet]
        public Task<IActionResult> Scoreboard(
            [FromQuery(Name = "affiliations[]")] int[] affiliations,
            [FromQuery(Name = "categories[]")] int[] categories,
            [FromQuery(Name = "clear")] string clear = "")
            => Scoreboard(
                isPublic: Contest.GetState() < ContestState.Finalized,
                isJury: false, clear == "clear", affiliations, categories);


        [HttpGet]
        [Route("/[area]/{cid}")]
        public async Task<IActionResult> Info()
        {
            ViewBag.Affiliations = await Context.FetchAffiliationsAsync();
            ViewBag.Categories = await Context.FetchCategoriesAsync();

            var fileInfo = io.GetFileInfo($"c{cid}/readme.html");
            ViewBag.Markdown = await fileInfo.ReadAsync();
            return View();
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [AuditPoint(Entities.AuditlogType.Team)]
        public async Task<IActionResult> Register()
        {
            if (ViewData.ContainsKey("HasTeam"))
            {
                StatusMessage = "Already registered";
                return RedirectToAction(nameof(Info));
            }

            if (!Contest.RegisterCategory.HasValue || User.IsInRole("Blocked"))
            {
                StatusMessage = "Error registration closed.";
                return RedirectToAction(nameof(Info));
            }

            string defaultAff = User.IsInRole("Student") ? "jlu" : "null";
            var affiliations = await Context.FetchAffiliationsAsync(false);
            var aff = affiliations.Values.SingleOrDefault(a => a.Abbreviation == defaultAff);
            if (aff == null) throw new System.ApplicationException("No default affiliation.");

            var userManager = HttpContext.RequestServices.GetRequiredService<IUserManager>();
            var user = await userManager.GetUserAsync(User);

            var team = await Context.CreateTeamAsync(
                users: new[] { user },
                team: new Team
                {
                    AffiliationId = aff.Id,
                    ContestId = Contest.Id,
                    CategoryId = Contest.RegisterCategory.Value,
                    RegisterTime = System.DateTimeOffset.Now,
                    Status = 0,
                    TeamName = User.GetNickName() ?? User.GetUserName(),
                });

            await HttpContext.AuditAsync("added", $"{team.TeamId}");
            StatusMessage = "Registration succeeded.";
            return RedirectToAction(nameof(Info));
        }
    }
}
