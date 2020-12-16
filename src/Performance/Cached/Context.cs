using Ccs.Entities;
using Ccs.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ccs.Contexts.Cached
{
    public class CachedContestContext : Immediate.ImmediateContestContext
    {
        private readonly IMemoryCache _cache;
        private readonly CachedContestContextIntervalOptions _options;

        public CachedContestContext(
            Contest contest,
            IMemoryCache cache,
            IServiceProvider serviceProvider,
            IOptions<CachedContestContextIntervalOptions> options) :
            base(contest, serviceProvider)
        {
            _cache = cache;
            _options = options.Value;
        }

        private Task<TValue> CacheAsync<TValue>(string t, TimeSpan? s, Func<Task<TValue>> f)
        {
            return _cache.GetOrCreateAsync($"Context({Contest.Id})::{t}", async entry =>
            {
                var value = await f();
                entry.AbsoluteExpirationRelativeToNow = s;
                return value;
            });
        }

        public override Task<IReadOnlyList<Language>> FetchLanguagesAsync()
        {
            return CacheAsync("Languages", _options.Language,
                async () => await base.FetchLanguagesAsync());
        }

        public override Task<IReadOnlyList<ProblemModel>> FetchProblemsAsync()
        {
            return CacheAsync("Problems", _options.Problem,
                async () => await base.FetchProblemsAsync());
        }

        public override Task<Team?> FindTeamByIdAsync(int teamId)
        {
            return CacheAsync($"Teams::Id({teamId})", _options.Team,
                async () => await base.FindTeamByIdAsync(teamId));
        }

        public override Task<Team?> FindTeamByUserAsync(int userId)
        {
            return CacheAsync($"Teams::User({userId})", _options.Team,
                async () => await base.FindTeamByUserAsync(userId));
        }
    }
}
