using Ccs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Area("Contest")]
    [Route("[area]/{cid:c(1)}/jury/balloon")]
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
            var model = await Context.FetchBalloonsAsync();
            return View(model);
        }


        [HttpGet("{id}/[action]")]
        public async Task<IActionResult> SetDone(int id)
        {
            await Context.SetBalloonDoneAsync(id);
            return RedirectToAction(nameof(List));
        }
    }
}
