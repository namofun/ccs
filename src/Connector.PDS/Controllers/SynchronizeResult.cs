using SatelliteSite.ContestModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xylab.Contesting.Connector.PlagiarismDetect.Models;
using Xylab.Contesting.Services;
using Xylab.PlagiarismDetect.Backend.Models;
using Xylab.PlagiarismDetect.Backend.Services;
using Xylab.Polygon.Entities;

namespace Xylab.Contesting.Connector.PlagiarismDetect.Controllers
{
    public class SynchronizeResult : LongRunningOperationResult
    {
        public SynchronizationOptionsModel Model { get; }

        public PlagiarismSet PlagiarismSet { get; }

        public SynchronizeResult(
            SynchronizationOptionsModel model,
            PlagiarismSet plagiarismSet)
            : base("text/html")
        {
            Model = model;
            PlagiarismSet = plagiarismSet;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var pds = GetService<IPlagiarismDetectService>();
            var ccs = GetContext<ISubmissionContext>();

            var problemIds = (Model.ChosenProblems ?? Array.Empty<int>()).ToHashSet();
            var problems = await ccs.ListProblemsAsync();
            var unusedProbIds = problemIds.Where(a => problems.Find(a) == null).ToList();
            var deadline = (ccs.Contest.StartTime + ccs.Contest.EndTime) ?? DateTimeOffset.Now;

            var langMapping = new Dictionary<string, string>
            {
                ["c"] = "cpp",
                ["cpp"] = "cpp",
                ["cpp11"] = "cpp",
                ["cpp14"] = "cpp",
                ["cpp17"] = "cpp",
                ["cpp98"] = "cpp",
                ["cpp2"] = "cpp",
                ["csharp"] = "csharp",
                ["java"] = "java",
                ["java8"] = "java",
                ["java11"] = "java",
                ["py2"] = "python",
                ["py3"] = "python",
                ["py"] = "python",
                ["python"] = "python",
            };

            if (unusedProbIds.Count > 0)
            {
                await WriteAsync(
                    "<p class=\"text-bold text-danger\">Unknown problem ID: " +
                    string.Join(", ", unusedProbIds) +
                    ", skipping...</p>");
            }

            var teams = await ccs.GetTeamNamesAsync();
            var usedProbIds = problemIds.Except(unusedProbIds).ToList();
            await WriteAsync($"<p>Processing for {problemIds.Count - unusedProbIds.Count} problems and {teams.Count} teams...</p>");

            int processed = 0;
            await WriteAsync("<pre>");
            foreach (var teamId in teams.Keys)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                await WriteAsync("team " + teamId + ":");

                var pds_submit = await pds.ListSubmissionsAsync(
                    setid: PlagiarismSet.Id,
                    exclusive_category: teamId);

                var sids = pds_submit.Select(a => a.Id).ToHashSet();

                var ccs_submit = await ccs.ListSolutionsAsync(
                    predicate: s => s.TeamId == teamId && s.Time <= deadline && !s.Ignored,
                    selector: (s, j) => new { s.Id, s.Language, s.Time, s.ProblemId, s.SourceCode, j.Status });

                foreach (var s in ccs_submit)
                {
                    if (sids.Contains(s.Id)) continue;
                    if (!problemIds.Contains(s.ProblemId)) continue;
                    if (s.Status == Verdict.CompileError) continue;
                    if (s.Status != Verdict.Accepted && ccs.Contest.RankingStrategy != CcsDefaults.RuleIOI) continue;
                    var prob = problems.Find(s.ProblemId);

                    if (!langMapping.ContainsKey(s.Language))
                    {
                        await WriteAsync($" s{s.Id} (skipped for language {s.Language}),");
                        continue;
                    }

                    await pds.SubmitAsync(new SubmissionCreation
                    {
                        Id = s.Id,
                        SetId = PlagiarismSet.Id,
                        InclusiveCategory = s.ProblemId,
                        ExclusiveCategory = teamId,
                        Name = $"{s.Status}, s{s.Id} from {teams[teamId]} (t{teamId}) on {prob.ShortName} - {prob.Title}",
                        Language = langMapping[s.Language],
                        Files = new List<SubmissionCreation.SubmissionFileCreation>
                        {
                            new SubmissionCreation.SubmissionFileCreation
                            {
                                FileName = "Main." + s.Language,
                                Content = s.SourceCode.UnBase64(),
                                FilePath = "Main." + s.Language,
                            }
                        }
                    });

                    await WriteAsync($" s{s.Id} (ok),");
                    processed++;
                }

                await WriteAsync(" done\n");
            }

            await WriteAsync("</pre>");

            await WriteAsync(
                "<p>" +
                "<span class=\"text-success\"><b>Synchronization finished</b></span>. " +
                $"{processed} submissions added to PDS." +
                "</p>");
        }
    }
}
