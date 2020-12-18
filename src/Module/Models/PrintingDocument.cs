using System;
using System.Text.Json.Serialization;

namespace SatelliteSite.ContestModule.Models
{
    public class PrintingDocument
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; set; }

        [JsonPropertyName("lang")]
        public string Language { get; set; }

        [JsonPropertyName("team")]
        public string Team { get; set; }

        [JsonPropertyName("filename")]
        public string FileName { get; set; }

        [JsonPropertyName("room")]
        public string Room { get; set; }

        [JsonPropertyName("processed")]
        public bool Processed { get; set; }

        [JsonPropertyName("done")]
        public bool Done { get; set; }

        [JsonPropertyName("sourcecode")]
        public string SourceCode { get; set; }
    }
}
