using Ccs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ccs.Entities
{
    /// <summary>
    /// The entity class for contest settings.
    /// </summary>
    public sealed class ContestSettings : IContestSettings
    {
        /// <inheritdoc />
        [JsonPropertyName("printing")]
        public bool PrintingAvailable { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("balloon")]
        public bool BalloonAvailable { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("event")]
        public bool EventAvailable { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("status")]
        public int StatusAvailable { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("languages")]
        public string[]? Languages { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("registration")]
        public Dictionary<string, int>? RegisterCategory { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("plagiarism-set")]
        public string? PlagiarismSet { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("restrict-ip")]
        public int? RestrictIp { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("restrict-ip-range")]
        public string[]? IpRanges { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("penalty-time")]
        public int? PenaltyTime { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("scoreboard-paging")]
        public bool? ScoreboardPaging { get; set; }

        /// <inheritdoc />
        [JsonPropertyName("prefer-gym-ui")]
        public bool? PreferGymUI { get; set; }

        /// <summary>
        /// Parse the settings out from the <paramref name="settingsJson"/>.
        /// </summary>
        /// <param name="settingsJson">The JSON text of settings.</param>
        /// <returns>The settings instance.</returns>
        public static ContestSettings Parse(string? settingsJson)
        {
            return settingsJson?.AsJson<ContestSettings?>() ?? new ContestSettings();
        }

        /// <inheritdoc />
        public ContestSettings Clone()
        {
            return new ContestSettings
            {
                BalloonAvailable = BalloonAvailable,
                EventAvailable = EventAvailable,
                Languages = Languages?.ToArray(),
                PlagiarismSet = PlagiarismSet,
                PrintingAvailable = PrintingAvailable,
                RegisterCategory = RegisterCategory?.ToDictionary(k => k.Key, v => v.Value),
                StatusAvailable = StatusAvailable,
                RestrictIp = RestrictIp,
                IpRanges = IpRanges,
                PenaltyTime = PenaltyTime,
                ScoreboardPaging = ScoreboardPaging,
                PreferGymUI = PreferGymUI,
            };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return JsonSerializer.Serialize(
                this,
                new JsonSerializerOptions
                {
                    IgnoreNullValues = true
                });
        }
    }
}
