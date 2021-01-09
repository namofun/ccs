using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IContestRepository _store;

        public ContestsController(IContestRepository store)
        {
            _store = store;
        }


        [HttpGet]
        public async Task<IActionResult> List(int page = 1)
        {
            var model = await _store.ListAsync(page);
            return View(model);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Add(int kind)
        {
            var c = await _store.CreateAndAssignAsync(kind, User);
            await HttpContext.AuditAsync("added", $"{c.Id}");
            return RedirectToAction("Home", "Jury", new { area = "Contest", cid = c.Id });
        }
    }
}
