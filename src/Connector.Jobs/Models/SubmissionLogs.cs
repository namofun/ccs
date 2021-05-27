using Polygon.Entities;
using System;
using System.Text.Json.Serialization;

namespace Ccs.Connector.Jobs.Models
{
    public class SubmissionLogs
    {
        [JsonPropertyName("id")]
        public int SubmissionId { get; set; }

        [JsonPropertyName("teamid")]
        public int TeamId { get; set; }

        [JsonPropertyName("probid")]
        public int ProblemId { get; set; }

        [JsonPropertyName("langid")]
        public string Language { get; set; }

        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; set; }

        [JsonPropertyName("verdict")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Verdict Verdict { get; set; }

        [JsonPropertyName("runs")]
        public string Runs { get; set; }

        [JsonPropertyName("valid")]
        public bool Valid { get; set; }

        [JsonIgnore]
        public string SourceCode { get; set; }
    }
}
