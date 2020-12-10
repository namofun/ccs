using Ccs.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ccs.Models
{
    public class ScoreboardModel
    {
        public IReadOnlyDictionary<int, Team>? Data { get; }

        public DateTimeOffset RefreshTime { get; }
    }
}
