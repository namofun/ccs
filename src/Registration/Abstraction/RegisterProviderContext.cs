using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

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
