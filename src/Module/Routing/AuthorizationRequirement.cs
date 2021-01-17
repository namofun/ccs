using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Routing
{
    public class ContestVisibleRequirement : IAuthorizationRequirement
    {
    }

    public class ContestJuryRequirement : IAuthorizationRequirement
    {
    }

    public class ContestTeamRequirement : IAuthorizationRequirement
    {
    }

    public class ContestAuthorizationHandler : IAuthorizationHandler
    {
        private readonly IHttpContextAccessor _accessor;

        public ContestAuthorizationHandler(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var feature = _accessor.HttpContext.Features.Get<IContestFeature>();

            if (feature?.Context != null)
            {
                foreach (var req in context.Requirements)
                {
                    bool verified = req switch
                    {
                        ContestVisibleRequirement _ => feature.IsPublic || feature.HasTeam || feature.IsJury,
                        ContestJuryRequirement _ => feature.IsJury,
                        ContestTeamRequirement _ => feature.HasTeam,
                        _ => false,
                    };

                    if (verified) context.Succeed(req);
                }
            }

            return Task.CompletedTask;
        }
    }
}
