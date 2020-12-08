using System;
using System.Text.Json.Serialization;

namespace Ccs.Events
{
    /// <summary>
    /// Submissions, a.k.a. attempts to solve problems in the contest.
    /// <a href="https://clics.ecs.baylor.edu/index.php?title=Contest_API_2020#Submissions">More detail</a>
    /// </summary>
    public class Submission : AbstractEvent
    {
        /// <summary>
        /// Identifier of the submission
        /// </summary>
        /// <remarks>Usable as a label, typically a low incrementing number</remarks>
        [JsonPropertyName("id")]
        public override string Id { get; }

        /// <summary>
        /// Identifier of the language submitted for
        /// </summary>
        [JsonPropertyName("language_id")]
        public string LanguageId { get; }

        /// <summary>
        /// Identifier of the problem submitted for
        /// </summary>
        [JsonPropertyName("problem_id")]
        public string ProblemId { get; }

        /// <summary>
        /// Identifier of the team that made the submission
        /// </summary>
        [JsonPropertyName("team_id")]
        public string TeamId { get; }

        /// <summary>
        /// Timestamp of when the submission was made
        /// </summary>
        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; }

        /// <summary>
        /// Contest relative time when the submission was made
        /// </summary>
        [JsonPropertyName("contest_time")]
        public TimeSpan ContestTime { get; }

        /// <summary>
        /// Code entry point for specific languages
        /// </summary>
        [JsonPropertyName("entry_point")]
        public string? EntryPoint { get; }

        /// <summary>
        /// Submission files, contained at the root of the archive
        /// </summary>
        /// <remarks>Only allowed mime type is <c>application/zip</c>.</remarks>
        [JsonPropertyName("files")]
        public FileMeta[] Files { get; }

        /// <inheritdoc />
        protected override string EndpointType => "submissions";

        /// <inheritdoc />
        protected override DateTimeOffset GetTime(string action) => Time;

        /// <summary>
        /// Construct a <see cref="Submission"/>.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="langid">The language ID.</param>
        /// <param name="submitid">The submission ID.</param>
        /// <param name="probid">The problem ID.</param>
        /// <param name="teamid">The team ID.</param>
        /// <param name="time">The submission time.</param>
        /// <param name="diff">The contest diff time.</param>
        public Submission(int cid, string langid, int submitid, int probid, int teamid, DateTimeOffset time, TimeSpan diff)
        {
            Id = $"{submitid}";
            LanguageId = langid;
            ProblemId = $"{probid}";
            TeamId = $"{teamid}";
            Time = time;
            ContestTime = diff;
            Files = new[] { new FileMeta(cid, submitid) };
        }

        /// <summary>
        /// Metadata for file archive.
        /// </summary>
        public sealed class FileMeta
        {
            /// <summary>
            /// The file href
            /// </summary>
            [JsonPropertyName("href")]
            public string Href { get; }

            /// <summary>
            /// The file mime type
            /// </summary>
            [JsonPropertyName("mime")]
            public string Mime { get; }

            /// <summary>
            /// Construct a <see cref="FileMeta"/>.
            /// </summary>
            /// <param name="cid">The contest ID.</param>
            /// <param name="submitid">The submission ID.</param>
            public FileMeta(int cid, int submitid)
            {
                Href = $"contests/{cid}/submissions/{submitid}/files";
                Mime = "application/zip";
            }
        }
    }
}
