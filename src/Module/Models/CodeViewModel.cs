using System;
using System.Collections.Generic;
using Xylab.Polygon.Entities;

namespace SatelliteSite.ContestModule.Models
{
    public class CodeViewModel
    {
        public Verdict Status { get; set; }
        public int? ExecuteTime { get; set; }
        public int? ExecuteMemory { get; set; }
        public string CompileError { get; set; }
        public int CodeLength { get; set; }
        public string Code { get; set; }
        public int JudgingId { get; set; }
        public int SubmissionId { get; set; }
        public string FileExtensions { get; set; }
        public string ProblemTitle { get; set; }
        public string LanguageName { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public IEnumerable<(JudgingRun, Testcase)> Details { get; set; }
    }
}