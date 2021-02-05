using System;
using System.Runtime.CompilerServices;

namespace Ccs.Entities
{
    /// <summary>
    /// The entity class for contests.
    /// </summary>
    public class Contest : IContestInformation
    {
        private ContestSettings? _settings;

        /// <inheritdoc />
        public int Id { get; set; }

        /// <inheritdoc />
        public string Name { get; set; } = "";

        /// <inheritdoc />
        public string ShortName { get; set; } = "DOMjudge";

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool IsPublic { get; set; }

        /// <inheritdoc />
        public int RankingStrategy { get; set; }

        /// <inheritdoc />
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

        /// <inheritdoc />
        TimeSpan? IContestTime.FreezeTime => FromSecondsToTimeSpan(FreezeTimeSeconds);

        /// <inheritdoc />
        TimeSpan? IContestTime.EndTime => FromSecondsToTimeSpan(EndTimeSeconds);

        /// <inheritdoc />
        TimeSpan? IContestTime.UnfreezeTime => FromSecondsToTimeSpan(UnfreezeTimeSeconds);

        /// <inheritdoc />
        IContestSettings IContestInformation.Settings => _settings ??= ContestSettings.Parse(SettingsJson);

        /// <summary>
        /// Converts the nullable double-typed time seconds to <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="time">The time in seconds.</param>
        /// <returns>The time in TimeSpan.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TimeSpan? FromSecondsToTimeSpan(double? time)
            => time.HasValue ? TimeSpan.FromSeconds(time.Value) : default(TimeSpan?);
    }
}
