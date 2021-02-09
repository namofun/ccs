using Ccs.Models;
using System;

namespace Ccs.Entities
{
    /// <summary>
    /// The entity class for contests.
    /// </summary>
    public class Contest
    {
        /// <inheritdoc cref="IContestInformation.Id" />
        public int Id { get; set; }

        /// <inheritdoc cref="IContestInformation.Name" />
        public string Name { get; set; } = "";

        /// <inheritdoc cref="IContestInformation.ShortName" />
        public string ShortName { get; set; } = "DOMjudge";

        /// <inheritdoc cref="IContestTime.StartTime" />
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// The freeze time (in seconds)
        /// </summary>
        public double? FreezeTimeSeconds { get; set; }

        /// <summary>
        /// The end time (in seconds)
        /// </summary>
        public double? EndTimeSeconds { get; set; }

        /// <summary>
        /// The unfreeze time (in seconds)
        /// </summary>
        public double? UnfreezeTimeSeconds { get; set; }

        /// <inheritdoc cref="IContestInformation.IsPublic" />
        public bool IsPublic { get; set; }

        /// <inheritdoc cref="IContestInformation.RankingStrategy" />
        public int RankingStrategy { get; set; }

        /// <inheritdoc cref="IContestInformation.Kind" />
        public int Kind { get; set; }

        /// <summary>
        /// The settings JSON in type of <see cref="ContestSettings"/>
        /// </summary>
        public string? SettingsJson { get; set; }

        /// <summary>
        /// The count of registered teams
        /// </summary>
        /// <remarks>This field is used for caching.</remarks>
        public int TeamCount { get; set; }

        /// <summary>
        /// The count of problems
        /// </summary>
        /// <remarks>This field is used for caching.</remarks>
        public int ProblemCount { get; set; }
    }
}
