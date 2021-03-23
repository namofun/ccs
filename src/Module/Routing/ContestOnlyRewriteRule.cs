using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace SatelliteSite.ContestModule.Routing
{
    public class ContestOnlyRewriteRule : IRewriteRule
    {
        private readonly MinimalSiteOptions _options;

        public ContestOnlyRewriteRule(IOptions<MinimalSiteOptions> options)
        {
            _options = options.Value;
        }

        public void ApplyRule(RewriteContext context)
        {
            var headers = context.HttpContext.Request.Headers;
            var request = context.HttpContext.Request;
            var features = context.HttpContext.Features;
            var items = context.HttpContext.Items;

            if (features.Get<IMinimalSiteFeature>() != null)
            {
                context.Result = RuleResult.SkipRemainingRules;
                return;
            }

            if (headers.TryGetValue("X-Contest-Id", out var contestIds)
                && _options.Validate(headers)
                && contestIds.Count == 1
                && int.TryParse(contestIds.Single(), out int cid))
            {
                string prefix = $"/contest/{cid}";
                features.Set<IMinimalSiteFeature>(new MinimalSiteFeature(cid, request.Path, prefix));
                request.Path = prefix + request.Path;
            }
            else
            {
                items["__SuppressOutboundRewriting"] = true;
            }
        }

        public RuleResult ApplyUrl(ActionContext context, ref string path)
        {
            var prefix = context.HttpContext.Features.Get<IMinimalSiteFeature>()?.Prefix;

            if (prefix == null || !path.StartsWith(prefix))
            {
                return RuleResult.ContinueRules;
            }
            else if (path.Length == prefix.Length)
            {
                path = "/";
                return RuleResult.ContinueRules;
            }
            else if (path[prefix.Length] == '/')
            {
                path = path[prefix.Length..];
                return RuleResult.ContinueRules;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
