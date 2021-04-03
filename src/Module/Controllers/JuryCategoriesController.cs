using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.ContestModule.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Authorize(Policy = "ContestIsJury")]
    [Route("[area]/{cid:c(7)}/jury/categories")]
    [AuditPoint(AuditlogType.Team)]
    public class JuryCategoriesController : JuryControllerBase<ITeamContext>
    {
        [HttpGet]
        public async Task<IActionResult> List()
        {
            return View(new JuryListCategoriesModel
            {
                Categories = await Context.ListCategoriesAsync(false),
                TeamCount = await Context.AggregateTeamsAsync(t => t.CategoryId, t => t.Count()),
            });
        }


        [HttpGet("{catid}")]
        public async Task<IActionResult> Detail(int catid)
        {
            var cat = await Context.FindCategoryAsync(catid, false);
            if (cat == null) return NotFound();

            return View(new JuryViewCategoryModel
            {
                Category = cat,
                Teams = await Context.ListTeamsAsync(t => t.Status == 1 && t.CategoryId == catid),
            });
        }
    }
}
