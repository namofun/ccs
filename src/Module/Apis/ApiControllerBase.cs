using MediatR;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xylab.Contesting.Models;
using Xylab.Contesting.Services;

namespace SatelliteSite.ContestModule.Apis
{
    /// <summary>
    /// The endpoints controller base to connect to CDS.
    /// </summary>
    public class ApiControllerBase<TContestContext> : Microsoft.AspNetCore.Mvc.ApiControllerBase
        where TContestContext : class, IContestContext
    {
        /// <summary>
        /// Context for contests
        /// </summary>
        protected TContestContext Context { get; private set; }

        /// <summary>
        /// Contest entity
        /// </summary>
        protected IContestInformation Contest => Context.Contest;

        /// <summary>
        /// Messaging center
        /// </summary>
        protected IMediator Mediator { get; private set; }

        /// <inheritdoc />
        public override async Task OnActionExecuting(ActionExecutingContext context)
        {
            if (!User.IsInRoles("Administrator,CDS"))
            {
                context.Result = Forbid();
                return;
            }

            if (context.RouteData.Values.TryGetValue("cid", out object __cid)
                && int.TryParse((string)__cid, out int cid))
            {
                var factory = HttpContext.RequestServices.GetRequiredService<ScopedContestContextFactory>();
                Context = (TContestContext)await factory.CreateAsync(cid, false);

                if (Context != null)
                {
                    var feature = HttpContext.RequestServices.GetRequiredService<IContestFeature>();
                    feature.Contextualize(Context);
                    HttpContext.Features.Set(feature);

                    HttpContext.Items[nameof(cid)] = cid;
                    Mediator = HttpContext.RequestServices.GetRequiredService<IMediator>();
                    return;
                }
            }

            context.Result = NotFound();
        }
    }
}
