#nullable enable
using Microsoft.AspNetCore.Http;
using System.Net;

namespace SatelliteSite.ContestModule.Routing
{
    public class MinimalSiteOptions
    {
        public string? Keyword { get; set; }

        public string? RealIpHeaderName { get; set; }

        public bool Validate(IHeaderDictionary headers)
        {
            return Keyword == null
                || (headers.TryGetValue("X-Contest-Keyword", out var values)
                    && values.Count == 1
                    && values[0] == Keyword);
        }

        public bool HasRealIp(IHeaderDictionary headers, out IPAddress? ip)
        {
            ip = null;
            return RealIpHeaderName != null
                && headers.TryGetValue(RealIpHeaderName, out var remoteAddr)
                && remoteAddr.Count == 1
                && IPAddress.TryParse(remoteAddr[0], out ip);
        }
    }
}
