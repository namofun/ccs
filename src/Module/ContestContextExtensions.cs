#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xylab.Contesting.Entities;
using Xylab.Contesting.Services;

namespace SatelliteSite.ContestModule
{
    /// <summary>
    /// Extensions for contest context.
    /// </summary>
    public static class ContestContextExtensions
    {
        private static readonly CultureInfo EnglishCulture
            = CultureInfo.GetCultureInfo(1033);

        /// <summary>
        /// Select two same property from the source enumerable.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="sources">The sources.</param>
        /// <param name="result1">The result1 selector.</param>
        /// <param name="result2">The result2 selector.</param>
        /// <returns>The enumerable for results.</returns>
        public static IEnumerable<TResult> SelectTwo<TSource, TResult>(
            this IEnumerable<TSource> sources,
            Func<TSource, TResult> result1,
            Func<TSource, TResult> result2)
        {
            foreach (var item in sources)
            {
                yield return result1(item);
                yield return result2(item);
            }
        }

        /// <summary>
        /// Figure out not null values.
        /// </summary>
        /// <typeparam name="T">The source type.</typeparam>
        /// <param name="sources">The sources.</param>
        /// <returns>The values.</returns>
        public static IEnumerable<T> NotNulls<T>(this IEnumerable<T?> sources) where T : struct
        {
            return sources.Where(a => a.HasValue).Select(a => a!.Value);
        }

        /// <summary>
        /// Find team by user ID.
        /// </summary>
        /// <param name="context">The contest context.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>The task for fetching team entity.</returns>
        public static async Task<Team?> FindTeamByUserAsync(this IContestContext context, int userId)
        {
            var member = await context.FindMemberByUserAsync(userId);
            if (member == null) return null;
            return await context.FindTeamByIdAsync(member.TeamId);
        }

        /// <summary>
        /// Format the state string.
        /// </summary>
        /// <param name="state">The contest state.</param>
        /// <param name="start">The start time.</param>
        /// <param name="end">The end time.</param>
        /// <returns>The status string.</returns>
        public static string ToString(this ContestState state, DateTimeOffset? start, TimeSpan? end)
        {
            if (state == ContestState.Frozen) state = ContestState.Started;
            return state switch
            {
                ContestState.NotScheduled => "scheduling",
                ContestState.Finalized => "final standings",
                ContestState.Ended => "contest over, waiting for results",
                ContestState.Started when (end?.TotalDays ?? 0) < 1 => $"starts: {start:HH:mm} - ends: {start + end:HH:mm}",
                ContestState.Started => $"{end?.Days} days, {end?.TotalHours - (end?.Days * 24)} hours",
                ContestState.ScheduledToStart when start!.Value.Date < DateTimeOffset.Now => $"scheduled to start on {start:HH:mm}",
                ContestState.ScheduledToStart => string.Format(EnglishCulture, "scheduled to start on {0:ddd, dd MMM yyyy HH:mm:ss} CST", start),
                _ => "unknown",
            };
        }
    }
}
