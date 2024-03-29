﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Xylab.Contesting.Entities;
using Xylab.Contesting.Models;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Models;

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

        public virtual string GetRunDetailsUrl(IUrlHelper urlHelper, int rid)
        {
            return urlHelper.Action(
                action: "RunDetails",
                controller: "JurySubmissions",
                values: new
                {
                    area = "Contest",
                    cid = Submission.ContestId,
                    submitid = SubmissionId,
                    judgingid = JudgingId,
                    runid = rid,
                });
        }

        public virtual string GetRunFileUrl(IUrlHelper urlHelper, int runid, string file)
        {
            return urlHelper.Action(
                action: "RunDetails",
                controller: "Submissions",
                values: new
                {
                    area = "Polygon",
                    probid = Submission.ProblemId,
                    submitid = SubmissionId,
                    judgingid = JudgingId,
                    runid = runid,
                    type = file,
                });
        }

        public virtual string GetTestcaseUrl(IUrlHelper urlHelper, int tcid, string file)
        {
            return urlHelper.Action(
                action: "Fetch",
                controller: "Testcases",
                values: new
                {
                    area = "Polygon",
                    probid = Submission.ProblemId,
                    testid = tcid,
                    filetype = file
                });
        }
    }
}
