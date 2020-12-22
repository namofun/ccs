using Ccs.Entities;
using Ccs.Models;
using Polygon.Entities;
using Polygon.Models;
using System.Collections.Generic;

namespace SatelliteSite.ContestModule.Models
{
    public class JuryViewSubmissionModel : ISubmissionDetail
    {
        public Submission Submission { get; set; }

        public Judging Judging { get; set; }

        public ICollection<Judging> AllJudgings { get; set; }

        public int SubmissionId => Submission.Id;

        public double RealTimeLimit => Problem.TimeLimit * Language.TimeFactor / 1000.0;

        public int JudgingId => Judging.Id;

        public IEnumerable<(JudgingRun, Testcase)> DetailsV2 { get; set; }

        public Verdict Verdict => Judging.Status;

        public string CompileError => Judging.CompileError;

        public bool CombinedRunCompare => Problem.Interactive;

        public string ServerName => Judging.Server;

        public ProblemModel Problem { get; set; }

        public Language Language { get; set; }

        public Team Team { get; set; }
    }
}
