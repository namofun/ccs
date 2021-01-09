using Ccs.Entities;
using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Dashboards
{
    [Area("Dashboard")]
    [Authorize(Roles = "Administrator,Teacher")]
    [Route("[area]/[controller]")]
    [AuditPoint(AuditlogType.Contest)]
    public class ContestsController : ViewControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> List(
            [FromServices] IContestRepository store)
        {
            var model = await store.ListAsync();
            return View(model);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Add(
            [FromServices] IContestRepository store,
            [FromServices] IUserManager userManager)
        {
            var user = await userManager.GetUserAsync(User);
            var c = await store.CreateAndAssignAsync(new Contest(), user);
            await HttpContext.AuditAsync("added", $"{c.Id}");
            return RedirectToAction("Home", "Jury", new { area = "Contest", cid = c.Id });
        }
    }
}
