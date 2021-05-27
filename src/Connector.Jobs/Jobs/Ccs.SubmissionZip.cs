using Ccs.Services;
using Jobs.Entities;
using Jobs.Models;
using Jobs.Services;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ccs.Connector.Jobs
{
    public class SubmissionZip : IJobExecutorProvider
    {
        private readonly IContestContextFactory _factory;
        private readonly IJobFileProvider _files;

        public SubmissionZip(
            IContestContextFactory factory,
            IJobFileProvider files)
        {
            _factory = factory;
            _files = files;
        }

        public string Type => "Ccs.SubmissionZip";

        public static JobDescription Create(Models.SubmissionArguments args, int owner)
        {
            return new JobDescription
            {
                SuggestedFileName = $"c{args.ContestId}-submissions.zip",
                Arguments = args.ToJson(),
                JobType = "Ccs.SubmissionZip",
                OwnerId = owner,
            };
        }

        public IJobExecutor Create(IServiceProvider serviceProvider)
        {
            return new Executor(_factory, serviceProvider, _files);
        }

        private class Executor : IJobExecutor
        {
            private readonly IContestContextFactory _factory;
            private readonly IServiceProvider _serviceProvider;
            private readonly IJobFileProvider _files;

            public Executor(
                IContestContextFactory factory,
                IServiceProvider serviceProvider,
                IJobFileProvider files)
            {
                _factory = factory;
                _serviceProvider = serviceProvider;
                _files = files;
            }

            public async Task<JobStatus> ExecuteAsync(string arguments, Guid guid, ILogger logger)
            {
                var args = arguments.AsJson<Models.SubmissionArguments>();
                var context = await _factory.CreateAsync(args.ContestId, _serviceProvider, true);
                var contest = context.Contest;

                if (context == null)
                {
                    logger.LogError("Unknown contest ID specified.");
                    return JobStatus.Failed;
                }

                if (contest.Kind == CcsDefaults.KindProblemset)
                {
                    logger.LogError("Export constraint failed.");
                    return JobStatus.Failed;
                }

                var now = DateTimeOffset.Now;
                var startTime = contest.StartTime ?? now;
                var endTime = (contest.StartTime + contest.EndTime) ?? now;

                logger.LogInformation("Fetching solutions...");

                var sol = await ((ISubmissionContext)context).ListSolutionsAsync(
                    predicate: Expr
                        .Of<Polygon.Entities.Submission>(null)
                        .CombineIf(!args.IncludeUpsolving, s => s.Time < endTime),

                    selector: (s, j) => new Models.SubmissionLogs
                    {
                        SourceCode = s.SourceCode,
                        SubmissionId = s.Id,
                        Language = s.Language,
                        ProblemId = s.ProblemId,
                        Runs = j.RunVerdicts,
                        Verdict = j.Status,
                        TeamId = s.TeamId,
                        Time = s.Time,
                        Valid = !s.Ignored,
                    });

                logger.LogInformation("Fetching workarounds...");
                var teams = await ((ITeamContext)context).GetScoreboardRowsAsync();
                var affs = await context.ListAffiliationsAsync();
                var orgs = await context.ListCategoriesAsync();
                var probs = (await context.ListProblemsAsync()).WhereIf(args.FilteredProblems != null, r => args.FilteredProblems.Contains(r.ProblemId));

                var contestWrite = new
                {
                    id = contest.Id,
                    name = contest.Name,
                    rule = contest.RankingStrategy switch { 0 => "xcpc", 1 => "oi", 2 => "cf", _ => null },
                    state = new Specifications.State(contest),
                    affiliations = affs.Values.WhereIf(args.FilteredAffiliations != null, r => args.FilteredAffiliations.Contains(r.Id)).Select(a => new Specifications.Organization(a)),
                    categories = orgs.Values.WhereIf(args.FilteredCategories != null, r => args.FilteredCategories.Contains(r.Id)).Select(a => new Specifications.Group(a)),
                    teams = teams
                        .WhereIf(args.FilteredAffiliations != null, r => args.FilteredAffiliations.Contains(r.AffiliationId))
                        .WhereIf(args.FilteredCategories != null, r => args.FilteredCategories.Contains(r.CategoryId))
                        .Select(t => new { id = t.TeamId, name = t.TeamName, affiliationid = t.AffiliationId, categoryid = t.CategoryId }),
                    problems = probs.Select(p => new Specifications.Problem(p))
                };

                var okTeams = contestWrite.teams.Select(t => t.id).ToHashSet();
                var okProbs = probs.Select(p => p.ProblemId).ToHashSet();
                sol = sol
                    .Where(s => okTeams.Contains(s.TeamId) && okProbs.Contains(s.ProblemId))
                    .Where(s => (s.Time < startTime && args.IncludePrevious)
                             || (s.Time >= startTime && s.Time < endTime && args.IncludeInner)
                             || (s.Time >= endTime && args.IncludeUpsolving))
                    .ToList();
                logger.LogInformation("Data loaded.");

                var options = new JsonSerializerOptions
                {
                    IgnoreNullValues = true,
                    WriteIndented = true,
                };

                using var memoryStream = new MemoryStream();
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    zipArchive.CreateEntryFromByteArray(
                        JsonSerializer.SerializeToUtf8Bytes(contestWrite, options),
                        "contest.json");

                    foreach (var s in sol)
                    {
                        zipArchive.CreateEntryFromByteArray(
                            Convert.FromBase64String(s.SourceCode),
                            $"s{s.SubmissionId}.{s.Language}");
                    }

                    zipArchive.CreateEntryFromByteArray(
                        JsonSerializer.SerializeToUtf8Bytes(sol, options),
                        "submissions.json");
                }

                memoryStream.Position = 0;
                await _files.WriteStreamAsync(guid + "/main", memoryStream);
                logger.LogInformation("Export succeeded.");
                return JobStatus.Finished;
            }
        }
    }
}
