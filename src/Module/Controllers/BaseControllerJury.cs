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
    public abstract class JuryControllerBase : ContestControllerBase
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

    /// <summary>
    /// Context-typed base controller for jury related things.
    /// </summary>
    public abstract class JuryControllerBase<TContext> : JuryControllerBase
        where TContext : class, Ccs.Services.IContestContext
    {
        /// <inheritdoc cref="ContestControllerBase.Context" />
        protected new TContext Context => base.Context as TContext;

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            if (Context == null) context.Result = NotFound();
        }
    }
}
