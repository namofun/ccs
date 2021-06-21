using Ccs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Authorize(Policy = "ContestIsBalloonRunner")]
    [Route("[area]/{cid:c(3)}/jury/balloon")]
    public class JuryBalloonsController : JuryControllerBase<IBalloonContext>
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (!Contest.Settings.BalloonAvailable)
            {
                context.Result = NotFound();
            }
        }


        [HttpGet]
        public async Task<IActionResult> List()
        {
            var model = await Context.ListAsync();
            return View(model);
        }


        [HttpGet("{id}/[action]")]
        public async Task<IActionResult> SetDone(int id)
        {
            await Context.HandleAsync(id);
            return RedirectToAction(nameof(List));
        }
    }
}
