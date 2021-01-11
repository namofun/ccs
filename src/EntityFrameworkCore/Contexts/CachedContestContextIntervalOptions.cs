using System;

namespace Ccs.Services
{
    public class CachedContestContextIntervalOptions
    {
        public TimeSpan Contest { get; set; } = TimeSpan.FromMinutes(5);

        public TimeSpan Language { get; set; } = TimeSpan.FromSeconds(10);

        public TimeSpan Problem { get; set; } = TimeSpan.FromMinutes(10);

        public TimeSpan Team { get; set; } = TimeSpan.FromMinutes(5);

        public TimeSpan Teams { get; set; } = TimeSpan.FromSeconds(10);
    }
}
