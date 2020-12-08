using System;
using System.Text.Json.Serialization;

namespace Ccs.Events
{
    /// <summary>
    /// Runs are judgements of individual test cases of a submission.
    /// <a href="https://clics.ecs.baylor.edu/index.php?title=Contest_API_2020#Runs">More detail</a>
    /// </summary>
    public class Run : AbstractEvent
    {
        /// <summary>
        /// Identifier of the run
        /// </summary>
        [JsonPropertyName("id")]
        public override string Id { get; }

        /// <summary>
        /// Identifier of the judgement this is part of
        /// </summary>
        [JsonPropertyName("judgement_id")]
        public string JudgementId { get; }

        /// <summary>
        /// Ordering of runs in the judgement
        /// </summary>
        /// <remarks>Must be different for every run in a judgement. Runs for the same test case must have the same ordinal. Must be between 1 and problem:<c>test_data_count</c>.</remarks>
        [JsonPropertyName("ordinal")]
        public int Ordinal { get; }

        /// <summary>
        /// The verdict of this judgement (i.e. a judgement type)
        /// </summary>
        [JsonPropertyName("judgement_type_id")]
        public string JudgementTypeId { get; }

        /// <summary>
        /// Absolute time when run completed
        /// </summary>
        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; }

        /// <summary>
        /// Contest relative time when run completed
        /// </summary>
        [JsonPropertyName("contest_time")]
        public TimeSpan ContestTime { get; }

        /// <summary>
        /// Run time in seconds
        /// </summary>
        [JsonPropertyName("run_time")]
        public double RunTime { get; }

        /// <inheritdoc />
        protected override string EndpointType => "runs";

        /// <inheritdoc />
        protected override DateTimeOffset GetTime(string action) => Time;

        /// <summary>
        /// Construct a <see cref="Run"/>.
        /// </summary>
        /// <param name="time">The finished time.</param>
        /// <param name="span">The relative time.</param>
        /// <param name="runid">The run ID.</param>
        /// <param name="jid">The judging ID.</param>
        /// <param name="v">The verdict.</param>
        /// <param name="rank">The testcase rank.</param>
        /// <param name="timems">The time in milliseconds.</param>
        public Run(DateTimeOffset time, TimeSpan span, int runid, int jid, Polygon.Entities.Verdict v, int rank, int timems)
        {
            Time = time;
            ContestTime = span;
            Id = $"{runid}";
            JudgementId = $"{jid}";
            JudgementTypeId = JudgementType.For(v) ?? "JE";
            Ordinal = rank;
            RunTime = timems / 1000.0;
        }
    }
}
