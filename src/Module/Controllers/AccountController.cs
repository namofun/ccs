using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SatelliteSite.ContestModule.Routing;
using SatelliteSite.IdentityModule.Models;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    [Authorize]
    [Area("Contest")]
    [Route("[area]/{cid:c(7)}/[controller]")]
    [AuditPoint(AuditlogType.User)]
    [MinimalSiteConstaint]
    public class AccountController : ViewControllerBase
    {
        private readonly ISignInManager _signInManager;

        public AccountController(ISignInManager signInManager)
        {
            _signInManager = signInManager;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var feature = HttpContext.Features.Get<IMinimalSiteFeature>();
            if (feature == null) context.Result = BadRequest();
            var ccs = HttpContext.Features.Get<IContestFeature>();

            ViewData["NavbarName"] = ccs.Context.Contest.Kind switch
            {
                0 => Ccs.CcsDefaults.NavbarPublic,
                1 => Ccs.CcsDefaults.NavbarGym,
                2 => Ccs.CcsDefaults.NavbarProblemset,
                _ => null,
            };

            ViewData["UseLightTheme"] = true;
            base.OnActionExecuting(context);
        }


        [HttpGet("/[area]/{cid:c(7)}/dashboard/{**slug}")]
        [HttpGet("/[area]/{cid:c(7)}/profile/{**slug}")]
        public IActionResult FeatureUnavailable()
        {
            return Message(
                title: "Feature unavailable",
                message: "This feature is not available in this domain. " +
                        "Please goto the primary site to continue operations.");
        }


        [HttpGet("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            if (InAjax) return RedirectToAction();

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return View();
        }


        [HttpPost("[action]")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return Redirect("/");
            }
            else if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(LoginWith2fa));
            }
            else if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }


        [HttpGet("[action]")]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return StatusCodePage(423);
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/");
        }


        [HttpGet("[action]")]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return StatusCodePage(403);
        }


        [HttpGet("login-with-2fa")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa()
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) return TwoFactorFail();

            return View(new LoginWith2faModel());
        }


        [HttpPost("login-with-2fa")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) return TwoFactorFail();

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, isPersistent: false, rememberClient: false);

            if (result.Succeeded)
            {
                return Redirect("/");
            }
            else if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return View();
            }
        }


        [HttpGet("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithRecoveryCode()
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) return TwoFactorFail();
            return View();
        }


        [HttpPost("[action]")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) return TwoFactorFail();

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);
            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                return Redirect("/");
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return View();
            }
        }


        private ShowMessageResult TwoFactorFail()
            => Message(
                title: "Two-factor authentication",
                message: "Unable to load two-factor authentication user.",
                type: BootstrapColor.danger);
    }
}
