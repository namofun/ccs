using Ccs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Route("[area]/{cid:c(7)}/jury/printings")]
    public class JuryPrintingsController : JuryControllerBase
    {
        IPrintingService Store { get; }

        public JuryPrintingsController(IPrintingService store) => Store = store;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            if (!Contest.PrintingAvailable) context.Result = NotFound();
        }


        [HttpGet]
        public async Task<IActionResult> List(int cid, int limit = 100)
        {
            var model = await Store.ListAsync(cid, limit);
            return View(model);
        }


        [HttpGet("{fid}/[action]")]
        public async Task<IActionResult> Done(int cid, int fid)
        {
            var entity = await Store.FindAsync(fid);
            if (entity == null || entity.ContestId != cid) return NotFound();

            bool result = await Store.SetStateAsync(entity, true);
            if (!result) return NotFound();
            return RedirectToAction(nameof(List));
        }


        [HttpGet("{fid}/[action]")]
        public async Task<IActionResult> Undone(int cid, int fid)
        {
            var entity = await Store.FindAsync(fid);
            if (entity == null || entity.ContestId != cid) return NotFound();

            bool result = await Store.SetStateAsync(entity, null);
            if (!result) return NotFound();
            return RedirectToAction(nameof(List));
        }


        [HttpGet("{fid}/[action]")]
        public async Task<IActionResult> Download(int cid, int fid)
        {
            var entity = await Store.FindAsync(fid, true);
            if (entity == null || entity.ContestId != cid) return NotFound();
            return File(entity.SourceCode, "text/plain", entity.FileName);
        }
    }
}
