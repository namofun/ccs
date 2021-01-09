using Ccs.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Route("[area]/{cid}/[action]")]
    public class PublicController : ContestControllerBase
    {
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
            ViewBag.Markdown = await Context.GetReadmeAsync();
            return View();
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [AuditPoint(AuditlogType.Team)]
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
            if (aff == null) throw new ApplicationException("No default affiliation.");

            var user = await UserManager.GetUserAsync(User);

            var team = await Context.CreateTeamAsync(
                users: new[] { user },
                team: new Team
                {
                    AffiliationId = aff.Id,
                    ContestId = Contest.Id,
                    CategoryId = Contest.RegisterCategory.Value,
                    RegisterTime = DateTimeOffset.Now,
                    Status = 0,
                    TeamName = User.GetNickName(),
                });

            await HttpContext.AuditAsync("added", $"{team.TeamId}");
            StatusMessage = "Registration succeeded.";
            return RedirectToAction(nameof(Info));
        }
    }
}
