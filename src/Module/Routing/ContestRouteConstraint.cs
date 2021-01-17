using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace SatelliteSite.ContestModule.Routing
{
    public class ContestRouteConstraint : IRouteConstraint
    {
        private readonly int _allowdMask;

        public ContestRouteConstraint() : this(7) { }

        public ContestRouteConstraint(int mask) => _allowdMask = mask;

        public bool Match(
            HttpContext httpContext,
            IRouter route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (routeDirection == RouteDirection.UrlGeneration) return true;
            var feature = httpContext.Features.Get<IContestFeature>();
            if (feature?.Context == null) return false;
            return (_allowdMask & (1 << feature.Context.Contest.Kind)) != 0;
        }
    }
}
