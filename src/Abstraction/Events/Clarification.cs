using System;
using System.Text.Json.Serialization;

namespace Ccs.Events
{
    /// <summary>
    /// Clarification message sent between teams and judges, a.k.a. clarification requests (questions from teams) and clarifications (answers from judges).
    /// <a href="https://clics.ecs.baylor.edu/index.php?title=Contest_API_2020#Clarifications">More detail</a>
    /// </summary>
    public class Clarification : AbstractEvent
    {
        /// <summary>
        /// Identifier of the clarification
        /// </summary>
        [JsonPropertyName("id")]
        public override string Id { get; }

        /// <summary>
        /// Identifier of team sending this clarification request, <c>null</c> if a clarification sent by jury
        /// </summary>
        [JsonPropertyName("from_team_id")]
        public string? FromTeamId { get; }

        /// <summary>
        /// Identifier of the team receiving this reply, <c>null</c> if a reply to all teams or a request sent by a team
        /// </summary>
        [JsonPropertyName("to_team_id")]
        public string? ToTeamId { get; }

        /// <summary>
        /// Identifier of clarification this is in response to, otherwise <c>null</c>
        /// </summary>
        [JsonPropertyName("reply_to_id")]
        public string? ReplyToId { get; }

        /// <summary>
        /// Identifier of associated problem, <c>null</c> if not associated to a problem
        /// </summary>
        [JsonPropertyName("problem_id")]
        public string? ProblemId { get; }

        /// <summary>
        /// Question or reply text
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; }

        /// <summary>
        /// Time of the question/reply
        /// </summary>
        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; }

        /// <summary>
        /// Contest time of the question/reply
        /// </summary>
        [JsonPropertyName("contest_time")]
        public TimeSpan ContestTime { get; }

        /// <inheritdoc />
        protected override string EndpointType => "clarifications";

        /// <inheritdoc />
        protected override DateTimeOffset GetTime(string action) => Time;

        /// <summary>
        /// Construct a <see cref="Clarification"/>.
        /// </summary>
        /// <param name="c">The clarification entity.</param>
        /// <param name="contestTime">The contest start time.</param>
        public Clarification(Entities.Clarification c, DateTimeOffset contestTime)
        {
            Time = c.SubmitTime;
            ContestTime = c.SubmitTime - contestTime;
            Id = $"{c.Id}";
            ReplyToId = c.ResponseToId?.ToString();
            FromTeamId = c.Sender?.ToString();
            ToTeamId = c.Recipient?.ToString();
            ProblemId = c.ProblemId?.ToString();
            Text = c.Body;
        }
    }
}
