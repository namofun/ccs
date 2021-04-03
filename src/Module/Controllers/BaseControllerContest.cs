using Ccs.Entities;
using Ccs.Models;
using Ccs.Registration;
using Ccs.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataTables;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Controllers
{
    /// <summary>
    /// Base controller for contest related things.
    /// </summary>
    public abstract class ContestControllerBase<TContestContext> : ViewControllerBase
        where TContestContext : class, IContestContext
    {
        private IUserManager _lazy_userManager;
        private IMediator _lazy_mediator;
        private IContestContextAccessor _accessor;

        /// <summary>
        /// Context for contest controlling
        /// </summary>
        protected internal TContestContext Context => (TContestContext)_accessor.Context;

        /// <summary>
        /// The contest entity
        /// </summary>
        protected internal IContestContextAccessor Contest => _accessor;

        /// <summary>
        /// The messaging center
        /// </summary>
        protected internal IMediator Mediator => _lazy_mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

        /// <summary>
        /// Gets the user manager.
        /// </summary>
        protected internal IUserManager UserManager => _lazy_userManager ??= HttpContext.RequestServices.GetRequiredService<IUserManager>();

        /// <summary>
        /// The team entity for current user
        /// </summary>
        protected internal Team Team => _accessor.Team;

        /// <summary>
        /// Whether the contest has not been started
        /// </summary>
        protected internal bool TooEarly => Contest.GetState() < ContestState.Started;

        /// <summary>
        /// Whether the contest has been finalized
        /// </summary>
        protected internal bool TooLate => Contest.GetState() == ContestState.Finalized;

        /// <summary>
        /// Returns a data table ajax result.
        /// </summary>
        /// <typeparam name="T">The display type.</typeparam>
        /// <param name="models">The list of entities.</param>
        /// <param name="draw">The draw ID.</param>
        /// <param name="count">The count of total.</param>
        /// <returns>The data table result.</returns>
        [NonAction]
        protected internal DataTableAjaxResult<T> DataTableAjax<T>(IEnumerable<T> models, int draw, int count)
        {
            return new DataTableAjaxResult<T>(models, draw, count);
        }

        /// <summary>
        /// Creates a <see cref="RegisterProviderContext"/>.
        /// </summary>
        /// <returns>The created <see cref="RegisterProviderContext"/>.</returns>
        protected internal RegisterProviderContext CreateRegisterProviderContext()
            => new RegisterProviderContext(_accessor, HttpContext, _lazy_userManager);

        /// <inheritdoc />
        [NonAction]
        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _accessor = HttpContext.Features.Get<IContestFeature>();

            // the event of contest state change
            await _accessor.Context.EnsureLastStateAsync();

            if (!Contest.IsPublic && !_accessor.JuryLevel.HasValue && !_accessor.HasTeam)
            {
                context.Result = NotFound();
                return;
            }

            await OnActionExecutingAsync(context);
            ViewData["ContestId"] = Contest.Id;

            if (context.Result == null)
            {
                await OnActionExecutedAsync(await next());
            }
        }

        /// <inheritdoc cref="Controller.OnActionExecuting(ActionExecutingContext)"/>
        /// <returns>A <see cref="Task"/> instance.</returns>
        [NonAction]
        public virtual Task OnActionExecutingAsync(ActionExecutingContext context)
        {
            OnActionExecuting(context);
            return Task.CompletedTask;
        }

        /// <inheritdoc cref="Controller.OnActionExecuted(ActionExecutedContext)"/>
        /// <returns>A <see cref="Task"/> instance.</returns>
        [NonAction]
        public virtual Task OnActionExecutedAsync(ActionExecutedContext context)
        {
            OnActionExecuted(context);
            return Task.CompletedTask;
        }
    }
}
