using Microsoft.AspNetCore.Mvc.ActionConstraints;
using System;

namespace SatelliteSite.ContestModule.Routing
{
    public sealed class MinimalSiteConstaintAttribute : Attribute, IActionConstraint
    {
        public int Order => 0;

        public bool Accept(ActionConstraintContext context)
        {
            return context.RouteContext.HttpContext.Features.Get<IMinimalSiteFeature>() != null;
        }
    }
}
