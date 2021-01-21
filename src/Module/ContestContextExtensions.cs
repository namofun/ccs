#nullable enable
using Ccs;
using Ccs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule
{
    /// <summary>
    /// Extensions for contest context.
    /// </summary>
    public static class ContestContextExtensions
    {
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
    }
}
