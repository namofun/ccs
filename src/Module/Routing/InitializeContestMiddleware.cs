using Ccs.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule.Routing
{
    public class InitializeContestMiddleware
    {
        private static readonly PathString _contestBase = new PathString("/contest");
        private static readonly PathString _problemsetBase = new PathString("/problemset");
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

            if (!(context.Request.Path.StartsWithSegments(_contestBase, out var remaining)
                || context.Request.Path.StartsWithSegments(_problemsetBase, out remaining))
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

            return InvokeAsync(context, cid);
        }

        private async Task InvokeAsync(HttpContext context, int cid)
        {
            var feature = context.RequestServices.GetRequiredService<IContestFeature>();
            var ctx = await _factory.CreateAsync(cid, context.RequestServices);
            feature.Contextualize(ctx);
            context.Features.Set<IContestFeature>(feature);

            if (feature.Context != null)
            {
                context.Items[nameof(cid)] = cid;

                // the event of contest state change
                await ctx.EnsureLastStateAsync();
            }

            await _next(context);
        }
    }
}
