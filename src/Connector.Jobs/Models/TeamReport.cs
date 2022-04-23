using System;
using System.Collections.Generic;
using System.Linq;
using Xylab.Contesting.Entities;
using Xylab.Contesting.Models;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Models;

namespace Xylab.Contesting.Connector.Jobs.Models
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
