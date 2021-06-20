using Ccs.Models;
using Ccs.Scoreboard;
using Ccs.Services;
using Jobs.Entities;
using Jobs.Models;
using Jobs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ccs.Connector.Jobs
{
    public class ScoreboardXlsx : IJobExecutorProvider
    {
        private readonly IContestContextFactory _factory;
        private readonly IJobFileProvider _files;

        public ScoreboardXlsx(
            IContestContextFactory factory,
            IJobFileProvider files)
        {
            _factory = factory;
            _files = files;
        }

        public string Type => "Ccs.ScoreboardXlsx";

        public static JobDescription Create(Models.ScoreboardArguments args, int owner)
        {
            return new JobDescription
            {
                SuggestedFileName = $"c{args.ContestId}-scoreboard" + (args.IncludeUpsolving ? "-upsolving" : "") + ".xlsx",
                Arguments = args.ToJson(),
                JobType = "Ccs.ScoreboardXlsx",
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
            private readonly IScoreboard _scoreboard;

            public Executor(
                IContestContextFactory factory,
                IServiceProvider serviceProvider,
                IJobFileProvider files)
            {
                _factory = factory;
                _serviceProvider = serviceProvider;
                _files = files;
                _scoreboard = serviceProvider.GetRequiredService<IScoreboard>();
            }

            public async Task<JobStatus> ExecuteAsync(string arguments, Guid guid, ILogger logger)
            {
                var args = arguments.AsJson<Models.ScoreboardArguments>();
                var context = await _factory.CreateAsync(args.ContestId, _serviceProvider, true);
                if (context == null)
                {
                    logger.LogError("Unknown contest ID specified.");
                    return JobStatus.Failed;
                }

                var contest = context.Contest;
                if (!contest.StartTime.HasValue
                    || contest.RankingStrategy == CcsDefaults.RuleCodeforces
                    || contest.Kind == CcsDefaults.KindProblemset)
                {
                    logger.LogError("Export constraint failed.");
                    return JobStatus.Failed;
                }

                logger.LogInformation("Calculating scoreboard...");

                DateTimeOffset? endTime = args.IncludeUpsolving
                    ? DateTimeOffset.Now
                    : (contest.StartTime + contest.EndTime);

                var raw = await RankingSolver.Select(contest)
                    .RefreshCache(
                        _scoreboard,
                        new Events.ScoreboardRefreshEvent(context, endTime));

                var rankCaches = raw.RankCache.ToDictionary(r => r.TeamId);
                var scoreCaches = raw.ScoreCache.ToLookup(r => r.TeamId);
                var teams1 = await ((ITeamContext)context).GetScoreboardRowsAsync();
                var teams = teams1.ToDictionary(
                    k => k.TeamId,
                    v => v.With(rankCaches.GetValueOrDefault(v.TeamId), scoreCaches[v.TeamId]));

                logger.LogInformation("Loading other things from database...");

                var affs = await context.ListAffiliationsAsync();
                var orgs = await context.ListCategoriesAsync();
                var probs = await context.ListProblemsAsync();
                var scb = new ScoreboardModel(contest.Id, teams, orgs, affs, probs, contest, RankingSolver.Select(contest));

                var board = new FullBoardViewModel(scb, false)
                {
                    FilteredAffiliations = args.FilteredAffiliations?.ToHashSet(),
                    FilteredCategories = args.FilteredCategories?.ToHashSet(),
                };

                logger.LogInformation("Data loaded.");

                using var workbook = OpenXmlScoreboard.Create(board, contest.Name);
                using var memoryStream = new MemoryStream();
                workbook.SaveAs(memoryStream);
                memoryStream.Position = 0;

                await _files.WriteStreamAsync(guid + "/main", memoryStream);
                logger.LogInformation("Export succeeded.");
                return JobStatus.Finished;
            }
        }
    }
}
