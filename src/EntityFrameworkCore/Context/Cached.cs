using Ccs.Entities;
using Ccs.Models;
using Ccs.Specifications;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public partial class CachedContestContext : ImmediateContestContext
    {
        private readonly IMemoryCache _cache;
        private readonly CachedContestContextIntervalOptions _options;

        public CachedContestContext(
            ContestWrapper contest,
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
            return _cache.GetOrCreateAsync($"Contest({Contest.Id})::{t}", async entry =>
            {
                var value = await f();
                entry.AbsoluteExpirationRelativeToNow = s;
                return value;
            });
        }

        private void Expire(string tag)
        {
            _cache.Remove($"Contest({Contest.Id})::{tag}");
        }

        protected override async Task WriteLastStateAsync(ContestState now, ContestState lastState)
        {
            await base.WriteLastStateAsync(now, lastState);
            Expire("State");
        }

        protected override Task<State> GetLastStateAsync()
        {
            return CacheAsync("State", _options.Contest,
                async () => await base.GetLastStateAsync());
        }
    }
}
