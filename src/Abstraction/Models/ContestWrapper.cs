using System;

namespace Ccs.Models
{
    /// <summary>
    /// The wrapper class for contest.
    /// </summary>
    public class ContestWrapper : IContestInformation
    {
        /// <inheritdoc />
        public int Id { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string ShortName { get; }

        /// <inheritdoc />
        public bool IsPublic { get; }

        /// <inheritdoc />
        public int RankingStrategy { get; }

        /// <inheritdoc />
        public int Kind { get; }

        /// <inheritdoc />
        public IContestSettings Settings { get; }

        /// <inheritdoc />
        public DateTimeOffset? StartTime { get; }

        /// <inheritdoc />
        public TimeSpan? FreezeTime { get; }

        /// <inheritdoc />
        public TimeSpan? EndTime { get; }

        /// <inheritdoc />
        public TimeSpan? UnfreezeTime { get; }

        /// <inheritdoc cref="Entities.Contest.TeamCount" />
        public int TeamCount { get; }

        /// <inheritdoc cref="Entities.Contest.ProblemCount" />
        public int ProblemCount { get; }

        /// <summary>
        /// Instantiate a <see cref="ContestWrapper"/>.
        /// </summary>
        /// <param name="entity">The source contest entity.</param>
        public ContestWrapper(Entities.Contest entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            ShortName = entity.ShortName;
            IsPublic = entity.IsPublic;
            RankingStrategy = entity.RankingStrategy;
            Kind = entity.Kind;
            Settings = Entities.ContestSettings.Parse(entity.SettingsJson);
            StartTime = entity.StartTime;
            TeamCount = entity.TeamCount;
            ProblemCount = entity.ProblemCount;
            FreezeTime = entity.FreezeTimeSeconds.HasValue ? TimeSpan.FromSeconds(entity.FreezeTimeSeconds.Value) : default(TimeSpan?);
            EndTime = entity.EndTimeSeconds.HasValue ? TimeSpan.FromSeconds(entity.EndTimeSeconds.Value) : default(TimeSpan?);
            UnfreezeTime = entity.UnfreezeTimeSeconds.HasValue ? TimeSpan.FromSeconds(entity.UnfreezeTimeSeconds.Value) : default(TimeSpan?);
        }
    }
}
