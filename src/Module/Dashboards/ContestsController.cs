using Ccs.Entities;
using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.IdentityModule.Services;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Dashboards
{
    [Area("Dashboard")]
    [Authorize(Roles = "Administrator,Teacher")]
    [Route("[area]/[controller]")]
    [AuditPoint(Entities.AuditlogType.Contest)]
    public class ContestsController : ViewControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> List(
            [FromServices] IContestStore store)
        {
            var model = await store.ListAsync();
            return View(model);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Add(
            [FromServices] IContestStore store,
            [FromServices] IUserManager userManager)
        {
            var c = await store.CreateAsync(new Contest());
            await HttpContext.AuditAsync("added", $"{c.Id}");

            var user = await userManager.GetUserAsync(User);
            await store.AssignJuryAsync(c, user);

            return RedirectToAction("Home", "Jury", new { area = "Contest", cid = c.Id });
        }
    }
}
