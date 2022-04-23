using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xylab.Contesting.Entities;
using Xylab.Contesting.Events;
using Xylab.Contesting.Models;
using Xylab.Contesting.Services;
using Xylab.Polygon.Events;

namespace Xylab.Contesting.Scoreboard
{
    /// <inheritdoc />
    public interface IRankingStrategyV2 : IRankingStrategy
    {
        /// <summary>
        /// The cell colors
        /// </summary>
        IReadOnlyList<(string StyleClass, string Name)> CellColors { get; }

        /// <summary>
        /// Statistics for the problem.
        /// </summary>
        /// <param name="model">The data model.</param>
        /// <param name="problem">The problem model.</param>
        /// <param name="time">The contest time model.</param>
        /// <returns>The statistics results.</returns>
        IReadOnlyList<(string Icon, string Title, string Value)> GetStatistics(ProblemStatisticsModel model, ProblemModel problem, IContestTime time);

        /// <summary>
        /// Statistics for the total solved.
        /// </summary>
        /// <param name="models">The data model.</param>
        /// <returns>The statistics results.</returns>
        int GetTotalSolved(ProblemStatisticsModel[] models);

        /// <summary>
        /// Gets the rank value.
        /// </summary>
        /// <param name="cache">The rank cache.</param>
        /// <param name="isPublic">Whether currently requires public.</param>
        /// <returns>The rank values.</returns>
        (int Points, int Penalty, int LastAc) GetRanks(RankCache cache, bool isPublic);

        /// <summary>
        /// Gets the score cell model.
        /// </summary>
        /// <param name="cache">The score cache.</param>
        /// <param name="isPublic">Whether currently requires public.</param>
        /// <returns>The cell model.</returns>
        ScoreCellModel ToCell(ScoreCache cache, bool isPublic);
    }


    public class NullRank : IRankingStrategyV2
    {
        public static NullRank Instance { get; } = new NullRank();

        public int Id => -1;
        public string Name => "NULL";
        public string FullName => "Unknown Rule";

        public IReadOnlyList<(string StyleClass, string Name)> CellColors
            => Array.Empty<(string, string)>();

        public Task Accept(IScoreboard store, IContestInformation contest, JudgingFinishedEvent args)
            => Task.CompletedTask;

        public Task CompileError(IScoreboard store, IContestInformation contest, JudgingFinishedEvent args)
            => Task.CompletedTask;

        public (int Points, int Penalty, int LastAc) GetRanks(RankCache cache, bool isPublic)
            => (0, 0, 0);

        public IReadOnlyList<(string Icon, string Title, string Value)> GetStatistics(ProblemStatisticsModel model, ProblemModel problem, IContestTime time)
            => Array.Empty<(string, string, string)>();

        public int GetTotalSolved(ProblemStatisticsModel[] models)
            => 0;

        public Task Pending(IScoreboard store, IContestInformation contest, SubmissionCreatedEvent args)
            => Task.CompletedTask;

        public Task<ScoreboardRawData> RefreshCache(IScoreboard store, ScoreboardRefreshEvent args)
            => Task.FromResult(new ScoreboardRawData(args.Contest.Id, Array.Empty<RankCache>(), Array.Empty<ScoreCache>()));

        public Task Reject(IScoreboard store, IContestInformation contest, JudgingFinishedEvent args)
            => Task.CompletedTask;

        public IEnumerable<IScoreboardRow> SortByRule(IEnumerable<IScoreboardRow> source, bool isPublic)
            => source;

        public ScoreCellModel ToCell(ScoreCache cache, bool isPublic)
            => new ScoreCellModel();
    }
}
