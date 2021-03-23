#nullable enable
using Microsoft.AspNetCore.Http;

namespace SatelliteSite.ContestModule.Routing
{
    public class MinimalSiteOptions
    {
        public string? Keyword { get; set; }

        public bool Validate(IHeaderDictionary headers)
        {
            return Keyword == null
                || (headers.TryGetValue("X-Contest-Keyword", out var values)
                    && values.Count == 1
                    && values[0] == Keyword);
        }
    }
}
