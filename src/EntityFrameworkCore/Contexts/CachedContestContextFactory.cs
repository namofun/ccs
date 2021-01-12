using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Ccs.Services
{
    public class CachedContestContextFactory : IContestContextFactory
    {
        private readonly IMemoryCache _cache;
        private readonly IOptions<CachedContestContextIntervalOptions> _options;

        public CachedContestContextFactory(IOptions<CachedContestContextIntervalOptions> options)
        {
            _cache = new MemoryCache(new MemoryCacheOptions { Clock = new SystemClock() });
            _options = options;
        }

        public async Task<IContestContext?> CreateAsync(int cid, IServiceProvider serviceProvider, bool requireProblems = true)
        {
            var contest = await _cache.GetOrCreateAsync(
                $"Contest({cid})::Core",
                async entry =>
                {
                    var cst = await serviceProvider.GetRequiredService<IContestRepository>().FindAsync(cid);
                    entry.AbsoluteExpirationRelativeToNow = _options.Value.Contest;
                    return cst;
                });

            if (contest == null) return null;
            return new CachedContestContext(contest, _cache, serviceProvider, _options);
        }
    }
}
