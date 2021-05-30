using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SatelliteSite.ContestModule.Routing
{
    public class MiddlewareConfigurator : IConfigureOptions<SubstrateOptions>
    {
        private readonly MinimalSiteOptions _options;

        public MiddlewareConfigurator(IOptions<MinimalSiteOptions> options)
        {
            _options = options.Value;
        }

        public void Configure(SubstrateOptions options)
        {
            options.PointBeforeRouting.Add(app => app.UseMiddleware<InitializeContestMiddleware>());
            options.PointBetweenAuth.Add(app => app.UseMiddleware<InitializeTeamMiddleware>());

            if (_options.RealIpHeaderName != null)
            {
                options.PointBeforeUrlRewriting.Add(app => app.UseMiddleware<RealIpV2Middleware>());
            }
        }
    }
}
