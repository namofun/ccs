using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using Xylab.Contesting.Services;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Authorize(Policy = "ContestIsBalloonRunner")]
    [Route("[area]/{cid:c(3)}/jury/printings")]
    public class JuryPrintingsController : JuryControllerBase<IContestContext>
    {
        IPrintingService Store { get; }

        public JuryPrintingsController(IPrintingService store) => Store = store;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            if (!Contest.Settings.PrintingAvailable) context.Result = NotFound();
        }


        [HttpGet]
        public async Task<IActionResult> List(int cid, int limit = 100)
        {
            var model = await Store.ListAsync(cid, limit);
            return View(model);
        }


        [HttpGet("{printid}/[action]")]
        public async Task<IActionResult> Done(int cid, int printid)
        {
            var entity = await Store.FindAsync(printid);
            if (entity == null || entity.ContestId != cid) return NotFound();

            bool result = await Store.SetStateAsync(entity, true);
            if (!result) return NotFound();
            return RedirectToAction(nameof(List));
        }


        [HttpGet("{printid}/[action]")]
        public async Task<IActionResult> Undone(int cid, int printid)
        {
            var entity = await Store.FindAsync(printid);
            if (entity == null || entity.ContestId != cid) return NotFound();

            bool result = await Store.SetStateAsync(entity, null);
            if (!result) return NotFound();
            return RedirectToAction(nameof(List));
        }


        [HttpGet("{printid}/[action]")]
        public async Task<IActionResult> Download(int cid, int printid)
        {
            var entity = await Store.FindAsync(printid, true);
            if (entity == null || entity.ContestId != cid) return NotFound();
            return File(entity.SourceCode, "text/plain", entity.FileName);
        }
    }
}
