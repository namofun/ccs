#nullable disable
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ccs.Specifications
{
    /// <summary>
    /// Scoreboard of the contest.
    /// <a href="https://clics.ecs.baylor.edu/index.php?title=Contest_API_2020#Scoreboard">More detail</a>
    /// </summary>
    /// <remarks>Since this is generated data, only the GET method is allowed here, irrespective of role.</remarks>
    public class Scoreboard
    {
        /// <summary>
        /// Identifier of the event after which this scoreboard was generated.
        /// </summary>
        /// <remarks>This must be identical to the argument <c>after_event_id</c>, if specified.</remarks>
        [JsonPropertyName("event_id")]
        public string EventId { get; set; }

        /// <summary>
        /// Time contained in the associated event
        /// </summary>
        /// <remarks>Implementation defined if the event has no associated time.</remarks>
        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// Contest time contained in the associated event
        /// </summary>
        /// <remarks>Implementation defined if the event has no associated contest time.</remarks>
        [JsonPropertyName("contest_time")]
        public TimeSpan ContestTime { get; set; }

        /// <summary>
        /// Identical data as returned by the contest state endpoint
        /// </summary>
        /// <remarks>This is provided here for ease of use and to guarantee the data is synchronized.</remarks>
        [JsonPropertyName("state")]
        public State State { get; set; }

        /// <summary>
        /// A list of rows of team with their associated scores
        /// </summary>
        [JsonPropertyName("rows")]
        public IEnumerable<Row> Rows { get; set; }

        /// <summary>
        /// The class for score object.
        /// </summary>
        [JsonConverter(typeof(ScoreJsonConverter))]
        public class Score
        {
            /// <summary>
            /// Whether the scoreboard is <c>pass-fail</c> or <c>score</c>
            /// </summary>
            [JsonIgnore]
            public bool IsPassFail { get; set; }

            /// <summary>
            /// Number of problems solved by the team
            /// </summary>
            /// <remarks>Required iff <c>contest:scoreboard_type</c> is <c>pass-fail</c></remarks>
            [JsonPropertyName("num_solved")]
            public int NumSolved { get; set; }

            /// <summary>
            /// Total penalty time accrued by the team
            /// </summary>
            /// <remarks>Required iff <c>contest:scoreboard_type</c> is <c>pass-fail</c></remarks>
            [JsonPropertyName("total_time")]
            public int TotalTime { get; set; }

            /// <summary>
            /// Total score of problems by the team
            /// </summary>
            /// <remarks>Required iff <c>contest:scoreboard_type</c> is <c>score</c></remarks>
            [JsonPropertyName("score")]
            public double PartialScore { get; set; }

            /// <summary>
            /// Time of last score improvement used for tiebreaking purposes
            /// </summary>
            [JsonPropertyName("time")]
            public int LastAccepted { get; set; }

            /// <summary>
            /// Construct a <see cref="Score"/>.
            /// </summary>
            public Score(bool isXcpc, int points, int penalty, int lastac)
            {
                if (isXcpc)
                {
                    IsPassFail = true;
                    TotalTime = penalty;
                    NumSolved = points;
                    LastAccepted = lastac;
                }
                else
                {
                    IsPassFail = false;
                    PartialScore = points;
                    LastAccepted = penalty;
                }
            }

            /// <inheritdoc />
            private class ScoreJsonConverter : JsonConverter<Score>
            {
                /// <inheritdoc />
                public override Score Read(
                    ref Utf8JsonReader reader,
                    Type typeToConvert,
                    JsonSerializerOptions options)
                    => throw new InvalidOperationException();

                /// <inheritdoc />
                public override void Write(
                    Utf8JsonWriter writer,
                    Score value,
                    JsonSerializerOptions options)
                {
                    writer.WriteStartObject();

                    if (value.IsPassFail)
                    {
                        writer.WriteNumber("num_solved", value.NumSolved);
                        writer.WriteNumber("total_time", value.TotalTime);
                        writer.WriteNumber("time", value.LastAccepted);
                    }
                    else
                    {
                        writer.WriteNumber("score", value.PartialScore);
                        writer.WriteNumber("time", value.LastAccepted);
                    }

                    writer.WriteEndObject();
                }
            }
        }

        /// <summary>
        /// The class for problem object.
        /// </summary>
        [JsonConverter(typeof(ProblemJsonConverter))]
        public class Problem
        {
            /// <summary>
            /// Whether the scoreboard is <c>pass-fail</c> or <c>score</c>
            /// </summary>
            [JsonIgnore]
            public bool IsPassFail { get; set; }

            /// <summary>
            /// Identifier of the problem
            /// </summary>
            [JsonPropertyName("problem_id")]
            public string ProblemId { get; set; }

            /// <summary>
            /// Label of the problem
            /// </summary>
            [JsonPropertyName("label")]
            public string Label { get; set; }

            /// <summary>
            /// Number of judged submissions (up to and including the first correct one)
            /// </summary>
            [JsonPropertyName("num_judged")]
            public int NumJudged { get; set; }

            /// <summary>
            /// Number of pending submissions (either queued or due to freeze)
            /// </summary>
            [JsonPropertyName("num_pending")]
            public int NumPending { get; set; }

            /// <summary>
            /// Whether the team solved this problem
            /// </summary>
            /// <remarks>Required iff <c>contest:scoreboard_type</c> is <c>pass-fail</c></remarks>
            [JsonPropertyName("solved")]
            public bool Solved { get; set; }

            /// <summary>
            /// The score of the last submission from team
            /// </summary>
            /// <remarks>
            /// Required iff <c>contest:scoreboard_type</c> is <c>score</c> and <c>solved</c> is missing.
            /// If missing or null defaults to 100 if solved is true and 0 if solved is false
            /// </remarks>
            [JsonPropertyName("score")]
            public double Score { get; set; }

            /// <summary>
            /// Minutes into the contest when this problem was solved by the team
            /// </summary>
            /// <remarks>Required iff <c>solved=true</c></remarks>
            [JsonPropertyName("time")]
            public int Time { get; set; }

            /// <summary>
            /// Whether the team is first to solve this problem
            /// </summary>
            [JsonPropertyName("first_to_solve")]
            public bool FirstToSolve { get; set; }

            /// <inheritdoc />
            private class ProblemJsonConverter : JsonConverter<Problem>
            {
                /// <inheritdoc />
                public override Problem Read(
                    ref Utf8JsonReader reader,
                    Type typeToConvert,
                    JsonSerializerOptions options)
                    => throw new InvalidOperationException();

                /// <inheritdoc />
                public override void Write(
                    Utf8JsonWriter writer,
                    Problem value,
                    JsonSerializerOptions options)
                {
                    writer.WriteStartObject();
                    writer.WriteString("label", value.Label);
                    writer.WriteString("problem_id", value.ProblemId);
                    writer.WriteNumber("num_judged", value.NumJudged);
                    writer.WriteNumber("num_pending", value.NumPending);

                    if (value.IsPassFail)
                    {
                        writer.WriteBoolean("solved", value.Solved);

                        if (value.Solved)
                        {
                            writer.WriteBoolean("first_to_solve", value.FirstToSolve);
                            writer.WriteNumber("time", value.Time);
                        }
                    }
                    else
                    {
                        writer.WriteNumber("score", value.Score);
                    }

                    writer.WriteEndObject();
                }
            }
        }

        /// <summary>
        /// The class for a row of scoreboard.
        /// </summary>
        public class Row
        {
            /// <summary>
            /// Rank of this team, 1-based and duplicate in case of ties
            /// </summary>
            [JsonPropertyName("rank")]
            public int Rank { get; set; }

            /// <summary>
            /// Identifier of the team
            /// </summary>
            [JsonPropertyName("team_id")]
            public string TeamId { get; set; }

            /// <summary>
            /// JSON object as specified in the rows below (for possible extension to other scoring methods)
            /// </summary>
            [JsonPropertyName("score")]
            public Score Score { get; set; }

            /// <summary>
            /// JSON array of problems with scoring data, see below for the specification of each element
            /// </summary>
            [JsonPropertyName("problems")]
            public IEnumerable<Problem> Problems { get; set; }
        }
    }
}
