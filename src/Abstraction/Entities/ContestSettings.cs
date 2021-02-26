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
                Languages = Languages?.ToArray(),
                PrintingAvailable = PrintingAvailable,
                RegisterCategory = RegisterCategory?.ToDictionary(k => k.Key, v => v.Value),
                StatusAvailable = StatusAvailable,
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
