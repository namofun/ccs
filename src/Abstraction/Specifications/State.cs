using System;
using System.Text.Json.Serialization;

namespace Ccs.Specifications
{
    /// <summary>
    /// Current state of the contest, specifying whether it's running, the scoreboard is frozen or results are final.
    /// <a href="https://clics.ecs.baylor.edu/index.php?title=Contest_API_2020#Contest_state">More detail</a>
    /// </summary>
    public class State : AbstractEvent
    {
        /// <summary>
        /// Identifier of the current contest
        /// </summary>
        [JsonIgnore]
        public override string Id { get; }

        /// <summary>
        /// Time when the contest actually started, or <c>null</c> if the contest has not started yet.
        /// </summary>
        /// <remarks>When set, this time must be equal to the contest <c>start_time</c>.</remarks>
        [JsonPropertyName("started")]
        public DateTimeOffset? Started { get; }

        /// <summary>
        /// Time when the scoreboard was frozen, or <c>null</c> if the scoreboard has not been frozen.
        /// </summary>
        /// <remarks>Required iff <c>scoreboard_freeze_duration</c> is present in the contest endpoint.</remarks>
        [JsonPropertyName("frozen")]
        public DateTimeOffset? Frozen { get; }

        /// <summary>
        /// Time when the contest ended, or <c>null</c> if the contest has not ended.
        /// </summary>
        /// <remarks>Must not be set if started is <c>null</c>.</remarks>
        [JsonPropertyName("ended")]
        public DateTimeOffset? Ended { get; }

        /// <summary>
        /// Time when the scoreboard was thawed (that is, unfrozen again), or <c>null</c> if the scoreboard has not been thawed.
        /// </summary>
        /// <remarks>Required iff <c>scoreboard_freeze_duration</c> is present in the contest endpoint. Must not be set if frozen is <c>null</c>.</remarks>
        [JsonPropertyName("thawed")]
        public DateTimeOffset? Thawed { get; }

        /// <summary>
        /// Time when the results were finalized, or <c>null</c> if results have not been finalized.
        /// </summary>
        /// <remarks>Must not be set if ended is <c>null</c>.</remarks>
        [JsonPropertyName("finalized")]
        public DateTimeOffset? Finalized { get; }

        /// <summary>
        /// Time after last update to the contest occurred, or <c>null</c> if more updates are still to come.
        /// </summary>
        /// <remarks>Setting this to non-<c>null</c> must be the very last change in the contest.</remarks>
        [JsonPropertyName("end_of_updates")]
        public DateTimeOffset? EndOfUpdates { get; }

        /// <inheritdoc />
        protected override string EndpointType => "state";

        /// <summary>
        /// Construct a <see cref="State"/>.
        /// </summary>
        /// <param name="ctx">The contest entity.</param>
        public State(Entities.Contest ctx)
        {
            Id = $"{ctx.Id}";

            switch (ctx.GetState())
            {
                case Entities.ContestState.Finalized:
                    EndOfUpdates = DateTimeOffset.Now;
                    Thawed = ctx.StartTime + ctx.UnfreezeTime;
                    Ended = ctx.StartTime + ctx.EndTime;
                    Finalized = Frozen.HasValue ? Thawed : Ended;
                    Frozen = ctx.StartTime + ctx.FreezeTime;
                    Started = ctx.StartTime;
                    break;
                case Entities.ContestState.Ended:
                    Ended = ctx.StartTime + ctx.EndTime;
                    Frozen = ctx.StartTime + ctx.FreezeTime;
                    Started = ctx.StartTime;
                    break;
                case Entities.ContestState.Frozen:
                    Frozen = ctx.StartTime + ctx.FreezeTime;
                    Started = ctx.StartTime;
                    break;
                case Entities.ContestState.Started:
                    Started = ctx.StartTime;
                    break;
                case Entities.ContestState.NotScheduled:
                case Entities.ContestState.ScheduledToStart:
                default:
                    break;
            }
        }
    }
}
