using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SatelliteSite
{
    public class Test46160Middleware
    {
        private readonly RequestDelegate _next;
        
        public Test46160Middleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            if (context.Connection.LocalPort == 46160)
            {
                context.Request.Headers["X-Contest-Id"] = "1";
                context.Request.Headers["X-Contest-Keyword"] = "hahaahahahahha";
            }
            else if (context.Connection.LocalPort == 46161)
            {
                context.Request.Headers["X-Contest-Keyword"] = "hahaahahahahha";
                context.Request.Headers["Jluds-hhh"] = "192.168.1.1";
            }

            return _next(context);
        }
    }
}
