using Polygon.Entities;
using System;
using System.Collections.Generic;

namespace SatelliteSite.ContestModule.Models
{
    public class SubmissionViewModel
    {
        public int SubmissionId { get; set; }

        public Verdict Verdict { get; set; }

        public DateTimeOffset Time { get; set; }

        public int TeamId { get; set; }

        public string TeamName { get; set; }

        public int Problem { get; set; }

        public string Language { get; set; }

        public IEnumerable<Verdict> Details { get; set; }

        public int Points { get; set; }

        public string CompilerOutput { get; set; }

        public string SourceCode { get; set; }

        public int JudgingId { get; set; }

        public int? ExecuteTime { get; set; }

        public int? ExecuteMemory { get; set; }
    }
}
