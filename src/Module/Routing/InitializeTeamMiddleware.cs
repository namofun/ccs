using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Routing
{
    public class InitializeTeamMiddleware
    {
        private readonly RequestDelegate _next;

        public InitializeTeamMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            return _next(context);
        }
    }
}
