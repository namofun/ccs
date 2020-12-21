using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    /// <summary>
    /// Base controller for jury related things.
    /// </summary>
    [Authorize]
    public abstract class JuryControllerBase : ContestControllerBase
    {
        /// <inheritdoc />
        public override Task OnActionExecutingAsync(ActionExecutingContext context)
        {
            // check the permission
            if (!ViewData.ContainsKey("IsJury"))
            {
                context.Result = Forbid();
                return Task.CompletedTask;
            }

            ViewData["InJury"] = true;
            return base.OnActionExecutingAsync(context);
        }

        /// <summary>
        /// Set the status message and redirect to jury home page.
        /// </summary>
        /// <param name="message">The status message.</param>
        /// <returns>The action result.</returns>
        protected IActionResult GoBackHome(string message)
        {
            StatusMessage = message;
            return RedirectToAction("Home", "Jury");
        }
    }
}
