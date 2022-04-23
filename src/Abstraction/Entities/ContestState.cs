namespace Xylab.Contesting.Entities
{
    /// <summary>
    /// The enum for contest state.
    /// </summary>
    public enum ContestState
    {
        /// <summary>
        /// Contest not scheduled
        /// </summary>
        /// <remarks>
        /// The contest time hasn't been scheduled.
        /// </remarks>
        NotScheduled,

        /// <summary>
        /// Contest not started
        /// </summary>
        /// <remarks>
        /// The contest time is set, but contest has not been started yet.
        /// </remarks>
        ScheduledToStart,

        /// <summary>
        /// Contest started without scroeboard frozen
        /// </summary>
        /// <remarks>
        /// The contest is started, but has not been ended or frozen.
        /// </remarks>
        Started,

        /// <summary>
        /// Contest started with scroeboard frozen
        /// </summary>
        /// <remarks>
        /// The contest scoreboard is frozen, but not ended.
        /// </remarks>
        Frozen,

        /// <summary>
        /// Contest ended with scoreboard frozen
        /// </summary>
        /// <remarks>
        /// The contest scoreboard is frozen and ended, waiting for thawing.
        /// </remarks>
        Ended,

        /// <summary>
        /// Contest finalized with scoreboard thawed
        /// </summary>
        /// <remarks>
        /// The contest is finalized, and the scoreboard shouldn't change any more.
        /// </remarks>
        Finalized,
    }
}
