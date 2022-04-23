using System;
using Xylab.Contesting.Entities;

namespace Xylab.Contesting.Models
{
    /// <summary>
    /// The extensions for entity interfaces.
    /// </summary>
    public static class EntityInterfaceExtensions
    {
        /// <summary>
        /// Gets the register category for the category name.
        /// </summary>
        /// <param name="settings">The contest settings.</param>
        /// <param name="categoryName">The category name.</param>
        /// <returns>The category ID, or null if not registrable.</returns>
        public static int? GetRegisterCategory(this IContestSettings settings, string categoryName)
        {
            if (settings.RegisterCategory == null) return null;
            if (string.IsNullOrWhiteSpace(categoryName)) return null;
            if (!settings.RegisterCategory.ContainsKey(categoryName)) return null;
            return settings.RegisterCategory[categoryName];
        }

        /// <summary>
        /// Gets a value indicating whether participants can register.
        /// </summary>
        /// <param name="settings">The contest settings.</param>
        /// <returns>The category ID, or null if not registrable.</returns>
        public static bool CanRegister(this IContestSettings settings)
        {
            if (settings.RegisterCategory == null) return false;
            return settings.RegisterCategory.Count != 0;
        }

        /// <summary>
        /// Gets the state of contest.
        /// </summary>
        /// <param name="time">The contest time.</param>
        /// <param name="nowTime">The current datetime.</param>
        /// <returns>The state of contest.</returns>
        public static ContestState GetState(IContestTime time, DateTimeOffset? nowTime = null)
        {
            // The implementation cannot depend on IContestTime.GetState().
            var now = nowTime ?? DateTimeOffset.Now;
            DateTimeOffset? start = time.StartTime;

            if (!start.HasValue)
            {
                return ContestState.NotScheduled;
            }

            if (start.Value > now)
            {
                // not started yet
                return ContestState.ScheduledToStart;
            }

            TimeSpan? end = time.EndTime, freeze = time.FreezeTime, unfreeze = time.UnfreezeTime;

            if (!end.HasValue)
            {
                // This is a special state. Usually this field
                // should not be null. But in some cases (like
                // problemset types), this may be null.
                return ContestState.Started;
            }

            var timeSpan = now - start.Value;

            if (freeze.HasValue)
            {
                // unfreezed
                if (unfreeze.HasValue && unfreeze.Value <= timeSpan)
                {
                    return ContestState.Finalized;
                }

                // ended, but not freezed
                if (end.Value <= timeSpan)
                {
                    return ContestState.Ended;
                }

                // freezed, but not ended
                if (freeze.Value <= timeSpan)
                {
                    return ContestState.Frozen;
                }

                // This contest may freeze later
                return ContestState.Started;
            }
            else if (end.Value <= timeSpan)
            {
                return ContestState.Finalized;
            }
            else
            {
                return ContestState.Started;
            }
        }
    }
}
