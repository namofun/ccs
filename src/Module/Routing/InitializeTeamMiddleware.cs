#nullable enable
using Ccs.Entities;
using Ccs.Services;
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

            bool restrictionFailed = false;
            if (feature.Context.Contest.Settings.RestrictIp is int restrictIp)
            {
                if ((restrictIp & 1) == 1)
                {
                    var ranges = await feature.Context.ListIpRangesAsync();
                    if (ranges == null) throw new NotImplementedException("Unknown configuration.");

                    bool anySatisfied = false;
                    var sourceIp = context.Connection.RemoteIpAddress;
                    for (int i = 0; i < ranges.Count && !anySatisfied; i++)
                    {
                        anySatisfied |= sourceIp.IsInRange(ranges[i].Address, ranges[i].Subnet);
                    }

                    restrictionFailed = !anySatisfied;
                }

                if ((restrictIp & 2) == 2)
                {
                    restrictionFailed |= context.Features.Get<IMinimalSiteFeature>() == null;
                }

                if ((restrictIp & 4) == 4)
                {
                    // TODO: Check last login IP
                }
            }

            feature.Authenticate(team, level);
            await _next(context);
        }
    }
}
