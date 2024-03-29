﻿using System;
using System.Text.Json.Serialization;
using Xylab.Contesting.Models;

namespace Xylab.Contesting.Specifications
{
    /// <summary>
    /// Current state of the contest, specifying whether it's running, the scoreboard is frozen or results are final.
    /// <a href="https://clics.ecs.baylor.edu/index.php?title=Contest_API_2020#Contest_state">More detail</a>
    /// </summary>
    public class State : AbstractEvent
    {
        private readonly DateTimeOffset _currentTime;

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
        public DateTimeOffset? Started { get; set; }

        /// <summary>
        /// Time when the scoreboard was frozen, or <c>null</c> if the scoreboard has not been frozen.
        /// </summary>
        /// <remarks>Required iff <c>scoreboard_freeze_duration</c> is present in the contest endpoint.</remarks>
        [JsonPropertyName("frozen")]
        public DateTimeOffset? Frozen { get; set; }

        /// <summary>
        /// Time when the contest ended, or <c>null</c> if the contest has not ended.
        /// </summary>
        /// <remarks>Must not be set if started is <c>null</c>.</remarks>
        [JsonPropertyName("ended")]
        public DateTimeOffset? Ended { get; set; }

        /// <summary>
        /// Time when the scoreboard was thawed (that is, unfrozen again), or <c>null</c> if the scoreboard has not been thawed.
        /// </summary>
        /// <remarks>Required iff <c>scoreboard_freeze_duration</c> is present in the contest endpoint. Must not be set if frozen is <c>null</c>.</remarks>
        [JsonPropertyName("thawed")]
        public DateTimeOffset? Thawed { get; set; }

        /// <summary>
        /// Time when the results were finalized, or <c>null</c> if results have not been finalized.
        /// </summary>
        /// <remarks>Must not be set if ended is <c>null</c>.</remarks>
        [JsonPropertyName("finalized")]
        public DateTimeOffset? Finalized { get; set; }

        /// <summary>
        /// Time after last update to the contest occurred, or <c>null</c> if more updates are still to come.
        /// </summary>
        /// <remarks>Setting this to non-<c>null</c> must be the very last change in the contest.</remarks>
        [JsonPropertyName("end_of_updates")]
        public DateTimeOffset? EndOfUpdates { get; set; }

        /// <inheritdoc />
        protected override string EndpointType => "state";

        /// <inheritdoc />
        protected override DateTimeOffset GetTime(string action) => _currentTime;

        /// <summary>
        /// The constructor used by JSON deserializers.
        /// </summary>
        public State() => Id = "unknown";

        /// <summary>
        /// Construct a <see cref="State"/>.
        /// </summary>
        /// <param name="ctx">The contest entity.</param>
        /// <param name="now">The current time, null if <see cref="DateTimeOffset.Now"/>.</param>
        public State(IContestInformation ctx, DateTimeOffset? now = null)
        {
            Id = $"{ctx.Id}";

            switch (ctx.GetState(now))
            {
                case Entities.ContestState.Finalized:
                    Started = ctx.StartTime;
                    Frozen = ctx.StartTime + ctx.FreezeTime;
                    Ended = ctx.StartTime + ctx.EndTime;
                    Thawed = ctx.StartTime + ctx.UnfreezeTime;
                    Finalized = Frozen.HasValue ? Thawed : Ended;
                    _currentTime = Finalized!.Value;
                    break;

                case Entities.ContestState.Ended:
                    Started = ctx.StartTime;
                    Frozen = ctx.StartTime + ctx.FreezeTime;
                    Ended = ctx.StartTime + ctx.EndTime;
                    _currentTime = Ended!.Value;
                    break;

                case Entities.ContestState.Frozen:
                    Started = ctx.StartTime;
                    Frozen = ctx.StartTime + ctx.FreezeTime;
                    _currentTime = Frozen!.Value;
                    break;

                case Entities.ContestState.Started:
                    Started = ctx.StartTime;
                    _currentTime = Started!.Value;
                    break;

                case Entities.ContestState.NotScheduled:
                case Entities.ContestState.ScheduledToStart:
                default:
                    break;
            }
        }

        /// <summary>
        /// Gets the enum state of current event state.
        /// </summary>
        /// <returns>The enum state.</returns>
        public Entities.ContestState GetState()
        {
            if (Finalized.HasValue)
            {
                return Entities.ContestState.Finalized;
            }
            else if (Ended.HasValue)
            {
                return Entities.ContestState.Ended;
            }
            else if (Frozen.HasValue)
            {
                return Entities.ContestState.Frozen;
            }
            else if (Started.HasValue)
            {
                return Entities.ContestState.Started;
            }
            else
            {
                return Entities.ContestState.ScheduledToStart;
            }
        }
    }
}
