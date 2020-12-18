﻿using System;
using System.Text.Json.Serialization;

namespace Ccs.Specifications
{
    /// <summary>
    /// Provides information on the current contest.
    /// <a href="https://clics.ecs.baylor.edu/index.php?title=Contest_API_2020#Contests">More detail</a>
    /// </summary>
    public class Contest : AbstractEvent
    {
        /// <summary>
        /// Identifier of the current contest
        /// </summary>
        [JsonPropertyName("id")]
        public override string Id { get; }

        /// <summary>
        /// Short logo name of the contest
        /// </summary>
        [JsonPropertyName("shortname")]
        public string ShortName { get; }

        /// <summary>
        /// Short display name of the contest
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; }

        /// <summary>
        /// Full name of the contest
        /// </summary>
        [JsonPropertyName("formal_name")]
        public string FormalName { get; }

        /// <summary>
        /// The scheduled start time of the contest
        /// </summary>
        /// <remarks>May be <c>null</c> if the start time is unknown or the countdown is paused.</remarks>
        [JsonPropertyName("start_time")]
        public DateTimeOffset? StartTime { get; }

        /// <summary>
        /// Length of the contest
        /// </summary>
        [JsonPropertyName("duration")]
        public TimeSpan Duration { get; }

        /// <summary>
        /// How long the scoreboard is frozen before the end of the contest
        /// </summary>
        [JsonPropertyName("scoreboard_freeze_duration")]
        public TimeSpan? ScoreboardFreezeDuration { get; }

        /// <summary>
        /// Penalty time for a wrong submission, in minutes
        /// </summary>
        [JsonPropertyName("penalty_time")]
        public int PenaltyTime { get; }

        /// <summary>
        /// The scheduled end time of the contest
        /// </summary>
        [JsonPropertyName("end_time")]
        public DateTimeOffset? EndTime { get; }
        
        /// <inheritdoc />
        protected override string EndpointType => "contests";

        /// <summary>
        /// Construct a <see cref="Contest"/>.
        /// </summary>
        /// <param name="c">The contest entity.</param>
        public Contest(Entities.Contest c)
        {
            FormalName = c.Name ?? "Demo Contest via Project CCS";
            Name = c.Name ?? "Demo Contest";
            ShortName = c.ShortName ?? "DOMjudge";
            Id = $"{c.Id}";
            PenaltyTime = 20;
            StartTime = c.StartTime;
            EndTime = c.StartTime + c.EndTime;
            Duration = EndTime - StartTime ?? TimeSpan.FromHours(5);
            ScoreboardFreezeDuration = c.EndTime - c.FreezeTime;
        }
    }
}