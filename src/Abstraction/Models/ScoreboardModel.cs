using System;
using System.Collections.Generic;

namespace Ccs.Models
{
    public class ScoreboardModel
    {
        public IReadOnlyDictionary<int, IScoreboardRow> Data { get; }

        public DateTimeOffset RefreshTime { get; }

        public ScoreboardModel(IReadOnlyDictionary<int, IScoreboardRow> data)
        {
            Data = data;
            RefreshTime = DateTimeOffset.Now;
        }

        public static ScoreboardModel Singleton { get; }
            = new ScoreboardModel(new Dictionary<int, IScoreboardRow>());
    }
}
