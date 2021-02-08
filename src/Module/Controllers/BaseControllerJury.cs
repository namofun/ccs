using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SatelliteSite.ContestModule.Controllers
{
    /// <summary>
    /// Base controller for jury related things.
    /// </summary>
    [Authorize]
    [Authorize(Policy = "ContestIsJury")]
    public abstract class JuryControllerBase<TContestContext> : ContestControllerBase<TContestContext>
        where TContestContext : class, Ccs.Services.IContestContext
    {
        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewData["InJury"] = true;
            ViewData["NavbarName"] = Ccs.CcsDefaults.JuryNavbar;
            ViewData["BigUrl"] = Url.Action("Home", "Jury");
            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Set the status message and redirect to jury home page.
        /// </summary>
        /// <param name="message">The status message.</param>
        /// <param name="action">The home page action.</param>
        /// <param name="controller">The home page controller.</param>
        /// <returns>The action result.</returns>
        protected virtual IActionResult GoBackHome(string message, string action = "Home", string controller = "Jury")
        {
            StatusMessage = message;
            return RedirectToAction(action, controller);
        }
    }
}
