using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SatelliteSite
{
    public class Test46160Middleware
    {
        private readonly int _port = 46160;
        private readonly RequestDelegate _next;
        
        public Test46160Middleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            if (context.Connection.LocalPort == _port)
            {
                context.Request.Headers["X-Contest-Id"] = "1";
                context.Request.Headers["X-Contest-Keyword"] = "hahaahahahahha";
            }

            return _next(context);
        }
    }
}
