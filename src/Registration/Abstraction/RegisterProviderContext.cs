using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Ccs.Registration
{
    /// <summary>
    /// The execution context for register provider.
    /// </summary>
    public sealed class RegisterProviderContext
    {
        /// <summary>
        /// Provides contest context.
        /// </summary>
        public IContestContext Context { get; }

        /// <summary>
        /// Provides user management.
        /// </summary>
        public IUserManager UserManager { get; }

        /// <summary>
        /// Provides http context.
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// Provides claims principal.
        /// </summary>
        public ClaimsPrincipal User
            => HttpContext.User;

        /// <summary>
        /// Gets the required service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The service instance.</returns>
        public TService GetRequiredService<TService>()
            => HttpContext.RequestServices.GetRequiredService<TService>();

        /// <summary>
        /// Instantiate an execution context for register provider.
        /// </summary>
        /// <param name="context">The contest context.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="httpContext">The http context.</param>
        public RegisterProviderContext(IContestContext context, IUserManager userManager, HttpContext httpContext)
        {
            Context = context;
            UserManager = userManager;
            HttpContext = httpContext;
        }
    }
}
