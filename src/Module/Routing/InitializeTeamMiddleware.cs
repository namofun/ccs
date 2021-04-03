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
                feature.Authenticate(null, null);
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
            {
                team = await feature.Context.FindTeamByUserAsync(uid);
            }

            JuryLevel? level = null;
            if (context.User.IsInRole("Administrator"))
            {
                level = JuryLevel.Administrator;
            }
            else
            {
                var juryList = await feature.Context.ListJuriesAsync();
                if (juryList.ContainsKey(uid)) level = juryList[uid].Item2;
            }

            feature.Authenticate(team, level);
            await _next(context);
        }
    }
}
