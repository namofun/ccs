using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SatelliteSite.ContestModule.Routing;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule
{
    public class RealIpV2Middleware
    {
        private readonly MinimalSiteOptions _options;
        private readonly RequestDelegate _next;
        private const string _realIpUsed = "X-RealIp-Used";

        public RealIpV2Middleware(
            RequestDelegate next,
            IOptions<MinimalSiteOptions> options)
        {
            _options = options.Value;
            _next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            if (_options.Validate(context.Request.Headers)
                && !context.Response.Headers.ContainsKey(_realIpUsed)
                && _options.HasRealIp(context.Request.Headers, out var ip))
            {
                context.Connection.RemoteIpAddress = ip;
                context.Response.Headers.Add(
                    _realIpUsed,
                    context.Connection.RemoteIpAddress.ToString());
            }

            return _next(context);
        }
    }
}
