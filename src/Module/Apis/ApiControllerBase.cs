using Ccs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Apis
{
    /// <summary>
    /// The endpoints controller base to connect to CDS.
    /// </summary>
    public class ApiControllerBase : Microsoft.AspNetCore.Mvc.ApiControllerBase
    {
        /// <summary>
        /// Context for contests
        /// </summary>
        protected IContestContext Context { get; private set; }

        /// <summary>
        /// Contest entity
        /// </summary>
        protected Ccs.Entities.Contest Contest => Context.Contest;

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
                Context = await HttpContext.CreateContestContextAsync(cid);
                if (Context != null)
                {
                    HttpContext.Items[nameof(cid)] = cid;
                    Mediator = HttpContext.RequestServices.GetRequiredService<IMediator>();
                    return;
                }
            }

            context.Result = NotFound();
        }
    }
}
