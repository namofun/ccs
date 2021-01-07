#nullable disable
using System;
using System.Collections.Generic;
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
        public class Score
        {
            /// <summary>
            /// Number of problems solved by the team
            /// </summary>
            [JsonPropertyName("num_solved")]
            public int NumSolved { get; set; }

            /// <summary>
            /// Total penalty time accrued by the team
            /// </summary>
            [JsonPropertyName("total_time")]
            public int TotalTime { get; set; }

            /// <summary>
            /// Construct a <see cref="Score"/>.
            /// </summary>
            /// <param name="a">The number solved.</param>
            /// <param name="b">The total penalty.</param>
            public Score(int a, int b)
            {
                NumSolved = a;
                TotalTime = b;
            }
        }

        /// <summary>
        /// The class for problem object.
        /// </summary>
        public class Problem
        {
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
            [JsonPropertyName("solved")]
            public bool Solved { get; set; }
        }

        /// <summary>
        /// The class for problem object.
        /// </summary>
        public class ProblemSolved : Problem
        {
            /// <summary>
            /// minutes into the contest when this problem was solved by the team
            /// </summary>
            /// <remarks>Required iff <c>solved=true</c></remarks>
            [JsonPropertyName("time")]
            public int Time { get; set; }

            /// <summary>
            /// Whether the team is first to solve this problem
            /// </summary>
            [JsonPropertyName("first_to_solve")]
            public bool FirstToSolve { get; set; }
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
