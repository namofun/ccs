using Ccs.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Routing
{
    public class InitializeContestMiddleware
    {
        private static readonly PathString _contestBase = new PathString("/contest");
        private readonly RequestDelegate _next;
        private readonly IContestContextFactory _factory;

        public InitializeContestMiddleware(RequestDelegate next, IContestContextFactory factory)
        {
            _next = next;
            _factory = factory;
        }

        public Task InvokeAsync(HttpContext context)
        {
            if (context.Features.Get<IContestFeature>() != null)
            {
                return _next(context);
            }

            if (!context.Request.Path.StartsWithSegments(_contestBase, out var remaining)
                || !remaining.HasValue)
            {
                return _next(context);
            }

            var cidSegment = remaining.Value.AsSpan().TrimStart('/');
            int firstSlash = cidSegment.IndexOf('/');
            if (firstSlash != -1) cidSegment = cidSegment.Slice(0, firstSlash);

            if (!int.TryParse(cidSegment, out int cid)
                || cid <= 0)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return Task.CompletedTask;
            }

            return ContestAsync(context, cid);
        }

        private async Task ContestAsync(HttpContext context, int cid)
        {
            var ctx = await _factory.CreateAsync(cid, context.RequestServices);
            context.Features.Set<IContestFeature>(new ContestFeature(ctx));
            await _next(context);
        }
    }
}
