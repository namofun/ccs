using System;
using System.Collections.Generic;
using Xylab.Contesting.Models;
using Xylab.Polygon.Entities;

namespace SatelliteSite.ContestModule.Models
{
    public class SubmissionViewModel
    {
        public int SubmissionId { get; set; }

        public Verdict Verdict { get; set; }

        public bool Skipped { get; set; }

        public DateTimeOffset Time { get; set; }

        public int TeamId { get; set; }

        public string TeamName { get; set; }

        public int ProblemId { get; set; }

        public string LanguageId { get; set; }

        public IEnumerable<Verdict> Details { get; set; }

        public int Points { get; set; }

        public string CompilerOutput { get; set; }

        public string SourceCode { get; set; }

        public int JudgingId { get; set; }

        public int? ExecuteTime { get; set; }

        public int? ExecuteMemory { get; set; }

        public ProblemModel Problem { get; set; }

        public Language Language { get; set; }

        public IEnumerable<(JudgingRun, Testcase)> Runs { get; set; }
    }
}
