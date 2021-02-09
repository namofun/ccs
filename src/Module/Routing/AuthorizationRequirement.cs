﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Routing
{
    public interface IContestAuthorizationHandler : IAuthorizationRequirement
    {
        Task HandleAsync(AuthorizationHandlerContext context, IContestFeature feature);
    }

    public class ContestVisibleRequirement : IContestAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context, IContestFeature feature)
        {
            if (feature.IsPublic || feature.HasTeam || feature.IsJury)
            {
                context.Succeed(this);
                return Task.CompletedTask;
            }

            var tenants = context.User.FindAll("tenant")
                .Select(t => int.TryParse(t.Value, out int v) ? v : default(int?))
                .NotNulls()
                .ToList();

            if (tenants.Count > 0)
            {
                return HandleCoreAsync();

                async Task HandleCoreAsync()
                {
                    if (await feature.Context.IsTenantVisibleAsync(tenants))
                    {
                        context.Succeed(this);
                    }
                }
            }
            else
            {
                return Task.CompletedTask;
            }
        }
    }

    public class ContestJuryRequirement : IContestAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context, IContestFeature feature)
        {
            if (feature.IsJury)
            {
                context.Succeed(this);
            }

            return Task.CompletedTask;
        }
    }

    public class ContestTeamRequirement : IContestAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context, IContestFeature feature)
        {
            if (feature.HasTeam)
            {
                context.Succeed(this);
            }

            return Task.CompletedTask;
        }
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
            if (feature?.Context == null)
            {
                return Task.CompletedTask;
            }

            var handlers = context.Requirements.OfType<IContestAuthorizationHandler>().ToList();
            if (handlers.Count == 0)
            {
                return Task.CompletedTask;
            }
            else if (handlers.Count == 1)
            {
                return handlers[0].HandleAsync(context, feature);
            }
            else
            {
                return HandleCoreAsync();

                async Task HandleCoreAsync()
                {
                    for (int i = 0; i < handlers.Count; i++)
                    {
                        await handlers[i].HandleAsync(context, feature);
                    }
                }
            }
        }
    }
}
