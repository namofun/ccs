using System;

namespace Ccs.Models
{
    /// <inheritdoc cref="IContestTime" />
    public class TimeOnlyModel : IContestTime
    {
        /// <inheritdoc />
        public DateTimeOffset? StartTime { get; set; }

        /// <inheritdoc />
        public TimeSpan? FreezeTime { get; set; }

        /// <inheritdoc />
        public TimeSpan? EndTime { get; set; }

        /// <inheritdoc />
        public TimeSpan? UnfreezeTime { get; set; }

        /// <inheritdoc />
        public Entities.ContestState GetState(DateTimeOffset? nowTime)
        {
            return EntityInterfaceExtensions.GetState(this, nowTime);
        }

        /// <summary>
        /// Initialize an time only model.
        /// </summary>
        public TimeOnlyModel()
        {
        }

        /// <summary>
        /// Initialize an time only model from existing time.
        /// </summary>
        public TimeOnlyModel(IContestTime time)
        {
            StartTime = time.StartTime;
            EndTime = time.EndTime;
            FreezeTime = time.FreezeTime;
            UnfreezeTime = time.UnfreezeTime;
        }
    }
}
