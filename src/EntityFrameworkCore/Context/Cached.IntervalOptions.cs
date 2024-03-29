﻿using System;

namespace Xylab.Contesting.Services
{
    public class CachedContestContextIntervalOptions
    {
        public TimeSpan Contest { get; set; } = TimeSpan.FromMinutes(5);

        public TimeSpan Language { get; set; } = TimeSpan.FromMinutes(5);

        public TimeSpan Problem { get; set; } = TimeSpan.FromMinutes(10);

        public TimeSpan Team { get; set; } = TimeSpan.FromMinutes(5);

        public TimeSpan Teams { get; set; } = TimeSpan.FromSeconds(10);

        public TimeSpan Scoreboard { get; set; } = TimeSpan.FromSeconds(3);

        public TimeSpan Statistics { get; set; } = TimeSpan.FromSeconds(30);
    }
}
