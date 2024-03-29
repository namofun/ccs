﻿using System;
using System.Text.Json.Serialization;

namespace Xylab.Contesting.Specifications
{
    /// <summary>
    /// Judgements for submissions in the contest.
    /// <a href="https://clics.ecs.baylor.edu/index.php?title=Contest_API_2020#Judgements">More detail</a>
    /// </summary>
    public class Judgement : AbstractEvent
    {
        /// <summary>
        /// Identifier of the judgement
        /// </summary>
        [JsonPropertyName("id")]
        public override string Id { get; }

        /// <summary>
        /// Identifier of the submission judged
        /// </summary>
        [JsonPropertyName("submission_id")]
        public string SubmissionId { get; }

        /// <summary>
        /// The verdict of this judgement
        /// </summary>
        [JsonPropertyName("judgement_type_id")]
        public string? JudgementTypeId { get; }

        /// <summary>
        /// Score for this judgement
        /// </summary>
        /// <remarks>Only relevant if <c>contest:scoreboard_type</c> is <c>score</c>. Defaults to 100 if missing</remarks>
        [JsonPropertyName("judgement_score")]
        public double? JudgementScore { get; }

        /// <summary>
        /// Absolute time when judgement started
        /// </summary>
        [JsonPropertyName("start_time")]
        public DateTimeOffset StartTime { get; }

        /// <summary>
        /// Contest relative time when judgement started
        /// </summary>
        [JsonPropertyName("start_contest_time")]
        public TimeSpan StartContestTime { get; }

        /// <summary>
        /// Absolute time when judgement completed
        /// </summary>
        [JsonPropertyName("end_time")]
        public DateTimeOffset? EndTime { get; }

        /// <summary>
        /// Contest relative time when judgement completed
        /// </summary>
        [JsonPropertyName("end_contest_time")]
        public TimeSpan? EndContestTime { get; }

        /// <summary>
        /// Maximum run time in seconds for any test case
        /// </summary>
        [JsonPropertyName("max_run_time")]
        public double? MaxRunTime { get; }

        /// <summary>
        /// If this judgement is valid and active
        /// </summary>
        /// <remarks>This is not CCS compatible property</remarks>
        [JsonPropertyName("valid")]
        public bool Valid { get; }

        /// <summary>
        /// Whether to override the time in rejudgings
        /// </summary>
        private readonly DateTimeOffset? _fakeTime;

        /// <inheritdoc />
        protected override string EndpointType => "judgements";

        /// <inheritdoc />
        protected override DateTimeOffset GetTime(string action) =>
            _fakeTime ?? (action == "create" ? StartTime : EndTime!).Value;

        /// <summary>
        /// Construct a <see cref="Judgement"/>.
        /// </summary>
        /// <param name="j">The judging entity.</param>
        /// <param name="contestTime">The contest start time.</param>
        /// <param name="verdict">The verdict to use instead.</param>
        /// <param name="fakeTime">The fake event time for rejudgings.</param>
        public Judgement(Xylab.Polygon.Entities.Judging j, DateTimeOffset contestTime, Xylab.Polygon.Entities.Verdict? verdict = null, DateTimeOffset? fakeTime = null)
        {
            _fakeTime = fakeTime;
            Id = $"{j.Id}";
            SubmissionId = $"{j.SubmissionId}";
            JudgementTypeId = JudgementType.For(verdict ?? j.Status);
            Valid = j.Active;
            StartContestTime = j.StartTime!.Value - contestTime;
            StartTime = j.StartTime!.Value;

            if (JudgementTypeId != null)
            {
                EndContestTime = j.StopTime!.Value - contestTime;
                EndTime = j.StopTime!.Value;
                JudgementScore = j.TotalScore;
                if (JudgementTypeId != "CE" && JudgementTypeId != "JE")
                    MaxRunTime = j.ExecuteTime / 1000.0;
            }
        }
    }
}
