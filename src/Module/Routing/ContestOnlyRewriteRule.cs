using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Rewrite;
using System;
using System.Linq;

namespace SatelliteSite.ContestModule.Routing
{
    public class ContestOnlyRewriteRule : IRewriteRule
    {
        public void ApplyRule(RewriteContext context)
        {
            var headers = context.HttpContext.Request.Headers;
            var request = context.HttpContext.Request;
            var items = context.HttpContext.Items;

            if (items.ContainsKey(this))
            {
                context.Result = RuleResult.SkipRemainingRules;
                return;
            }

            if (headers.TryGetValue("X-Contest-Id", out var contestIds)
                && contestIds.Count == 1
                && int.TryParse(contestIds.Single(), out int cid))
            {
                string prefix = $"/contest/{cid}";
                items[this] = prefix;
                request.Path = prefix + request.Path;
            }
            else
            {
                items["__SuppressOutboundRewriting"] = true;
            }
        }

        public RuleResult ApplyUrl(ActionContext context, ref string path)
        {
            if (!(context.HttpContext.Items[this] is string prefix))
                return RuleResult.ContinueRules;

            if (path.StartsWith(prefix))
            {
                if (path.Length == prefix.Length)
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

            return RuleResult.ContinueRules;
        }
    }
}
