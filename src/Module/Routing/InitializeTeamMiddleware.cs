#nullable enable
using Ccs.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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
            var feature = context.Features.Get<IContestFeature>();

            if (feature?.Context == null || feature.Authenticated)
            {
                return _next(context);
            }
            else if (!context.User.IsSignedIn())
            {
                feature.Authenticate(null, false);
                return _next(context);
            }
            else
            {
                return InvokeAsync(context, feature);
            }
        }

        private async Task InvokeAsync(HttpContext context, IContestFeature feature)
        {
            Team? team = null;
            if (int.TryParse(context.User.GetUserId(), out int uid))
                team = await feature.Context!.FindTeamByUserAsync(uid);

            bool isJury = context.User.IsInRole("Administrator") ||
                (await feature.Context!.FetchJuryAsync()).ContainsKey(uid);

            feature.Authenticate(team, isJury);
            await _next(context);
        }
    }
}
