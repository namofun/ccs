using System;

namespace Ccs.Entities
{
    /// <summary>
    /// The entity class for contests.
    /// </summary>
    public class Contest
    {
        /// <summary>
        /// The contest ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The contest title
        /// </summary>
        public string Name { get; set; } = "Contest Regular Round";

        /// <summary>
        /// The contest short name
        /// </summary>
        public string ShortName { get; set; } = "DOMjudge";

        /// <summary>
        /// The start time
        /// </summary>
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// The freeze time
        /// </summary>
        public TimeSpan? FreezeTime { get; set; }

        /// <summary>
        /// The end time
        /// </summary>
        public TimeSpan? EndTime { get; set; }

        /// <summary>
        /// The unfreeze time
        /// </summary>
        public TimeSpan? UnfreezeTime { get; set; }

        /// <summary>
        /// The ranking strategy
        /// </summary>
        /// <remarks>
        /// This represent the kind of ranking strategy.
        /// <list type="bullet"><c>0</c>: XCPC Rules</list>
        /// <list type="bullet"><c>1</c>: IOI Rules</list>
        /// <list type="bullet"><c>2</c>: Codeforces Rules</list>
        /// </remarks>
        public int RankingStrategy { get; set; }

        /// <summary>
        /// Whether this contest is public
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Whether printing is available
        /// </summary>
        public bool PrintingAvailable { get; set; }

        /// <summary>
        /// Whether balloon is available
        /// </summary>
        public bool BalloonAvailable { get; set; }

        /// <summary>
        /// Default self-register category
        /// </summary>
        public int? RegisterCategory { get; set; }

        /// <summary>
        /// The flags for contest kind
        /// </summary>
        /// <remarks>
        /// This represent the kind of contest.
        /// <list type="bullet"><c>0</c>: Normal contest with DOMjudge UI</list>
        /// <list type="bullet"><c>1</c>: Practice contest with Codeforces UI</list>
        /// <list type="bullet"><c>2</c>: Problem set with legacy OJ UI</list>
        /// </remarks>
        public int Kind { get; set; }

        /// <summary>
        /// The submission status availability
        /// </summary>
        /// <remarks>
        /// This represent the status of submission status.
        /// <list type="bullet"><c>0</c>: Unavailable</list>
        /// <list type="bullet"><c>1</c>: Available</list>
        /// <list type="bullet"><c>2</c>: Available when accepted</list>
        /// </remarks>
        public int StatusAvailable { get; set; }

        /// <summary>
        /// The available languages for submitting
        /// </summary>
        /// <remarks>
        /// <para>
        /// When <c>null</c>, the allowed languages list comes from <see cref="Polygon.Entities.Language.AllowSubmit"/>.
        /// </para>
        /// <para>
        /// Otherwise, a string of JSON array like <c>["c","cpp","java","python3"]</c>.
        /// If such language ID doesn't exist, the language ID will be ignored.
        /// </para>
        /// </remarks>
        public string? Languages { get; set; }

        /// <summary>
        /// Get the state of contest.
        /// </summary>
        /// <param name="nowTime">The current datetime.</param>
        /// <returns>The state of contest.</returns>
        public ContestState GetState(DateTimeOffset? nowTime = null)
        {
            var now = nowTime ?? DateTimeOffset.Now;

            if (!StartTime.HasValue)
                return ContestState.NotScheduled;
            if (StartTime.Value > now)
                return ContestState.ScheduledToStart;
            if (!EndTime.HasValue)
                return ContestState.Started;

            var timeSpan = now - StartTime.Value;

            if (FreezeTime.HasValue)
            {
                if (UnfreezeTime.HasValue && UnfreezeTime.Value < timeSpan)
                    return ContestState.Finalized;
                if (EndTime.Value < timeSpan)
                    return ContestState.Ended;
                if (FreezeTime.Value < timeSpan)
                    return ContestState.Frozen;
                return ContestState.Started;
            }
            else
            {
                if (EndTime.Value < timeSpan)
                    return ContestState.Finalized;
                return ContestState.Started;
            }
        }
    }
}
