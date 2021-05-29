using Ccs.Entities;
using Ccs.Models;
using Polygon.Entities;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ccs.Connector.Jobs.Models
{
    public class TeamReport
    {
        public int ContestId { get; set; }

        public string ContestName { get; set; }

        public int Rule { get; set; }

        public int TeamId { get; set; }

        public string TeamName { get; set; }

        public ProblemCollection Problems { get; set; }

        public IEnumerable<ScoreCache> ScoreCaches { get; set; }

        public RankCache RankCache { get; set; }

        public DateTimeOffset StartTime { get; set; }

        public DateTimeOffset EndTime { get; set; }

        public ILookup<int, Solution> Solutions { get; set; }

        public IReadOnlyDictionary<int, (int, string)> SourceCodes { get; set; }

        public IReadOnlyDictionary<int, IEnumerable<(JudgingRun, Testcase)>> Details { get; set; }
    }
}
