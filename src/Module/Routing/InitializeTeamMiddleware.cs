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
                feature.Authenticate(null, null, false);
                return _next(context);
            }
            else
            {
                return InvokeAsync(context, feature);
            }
        }

        private async Task InvokeAsync(HttpContext context, IContestFeature feature)
        {
            Member? member = null;
            Team? team = null;
            if (int.TryParse(context.User.GetUserId(), out int uid))
            {
                member = await feature.Context.FindMemberByUserAsync(uid);
                if (member != null) team = await feature.Context.FindTeamByIdAsync(member.TeamId);
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
            if (team != null && member != null
                && feature.Context.Contest.Kind == Ccs.CcsDefaults.KindDom
                && feature.Context.Contest.Settings.RestrictIp is int restrictIp)
            {
                var clientIp = context.Connection.RemoteIpAddress ?? System.Net.IPAddress.Parse("255.255.255.255");
                if (clientIp.IsIPv4MappedToIPv6) clientIp = clientIp.MapToIPv4();

                if ((restrictIp & 1) == 1)
                {
                    var ranges = await feature.Context.ListIpRangesAsync();
                    if (ranges == null) throw new NotImplementedException("Unknown configuration.");

                    bool anySatisfied = false;
                    for (int i = 0; i < ranges.Count && !anySatisfied; i++)
                    {
                        anySatisfied |= clientIp.IsInRange(ranges[i].Address, ranges[i].Subnet);
                    }

                    restrictionFailed |= !anySatisfied;
                }

                if ((restrictIp & 2) == 2)
                {
                    restrictionFailed |= context.Features.Get<IMinimalSiteFeature>() == null;
                }

                if ((restrictIp & 4) == 4)
                {
                    var nowIp = clientIp.ToString();
                    if (member.LastLoginIp != null)
                    {
                        restrictionFailed |= nowIp != member.LastLoginIp;
                    }
                    else
                    {
                        await ((ITeamContext)feature.Context)
                            .UpdateMemberAsync(
                                member,
                                m => new Member { LastLoginIp = nowIp });
                    }
                }
            }

            feature.Authenticate(team, level, restrictionFailed);
            await _next(context);
        }
    }
}
