using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule.Models;
using System.Linq;
using System.Threading.Tasks;
using Xylab.Contesting.Services;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Authorize(Policy = "ContestIsJury")]
    [Route("[area]/{cid:c(7)}/jury/affiliations")]
    [AuditPoint(AuditlogType.Team)]
    public class JuryAffiliationsController : JuryControllerBase<ITeamContext>
    {
        [HttpGet]
        public async Task<IActionResult> List()
        {
            return View(new JuryListAffiliationsModel
            {
                Affiliations = await Context.ListAffiliationsAsync(false),
                AllowedTenants = await Context.GetVisibleTenantsAsync(),
                TeamCount = await Context.AggregateTeamsAsync(t => t.AffiliationId, t => t.Count()),
            });
        }


        [HttpGet("{affid}")]
        public async Task<IActionResult> Detail(int affid)
        {
            var aff = await Context.FindAffiliationAsync(affid, false);
            if (aff == null) return NotFound();

            return View(new JuryViewAffiliationModel
            {
                Affiliation = aff,
                Teams = await Context.ListTeamsAsync(t => t.Status == 1 && t.AffiliationId == affid),
            });
        }


        [HttpGet("{affid}/[action]")]
        public async Task<IActionResult> ToggleVisibility(int affid)
        {
            var aff = await Context.FindAffiliationAsync(affid, false);
            if (aff == null) return NotFound();

            if (Contest.IsPublic)
            {
                StatusMessage = "The contest is public. Ice bear don't need to toggle the visibility.";
                return RedirectToAction(nameof(List));
            }

            var current = await Context.IsTenantVisibleAsync(new[] { affid });

            if (!current)
            {
                await Context.AllowTenantAsync(aff);
            }
            else
            {
                await Context.DisallowTenantAsync(aff);
            }

            StatusMessage = $"The visibility of this contest for {aff.Name} has been turned {(current ? "off" : "on")}.";
            return RedirectToAction(nameof(List));
        }
    }
}
