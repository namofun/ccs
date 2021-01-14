#nullable enable
using Ccs;
using Ccs.Entities;
using Ccs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SatelliteSite.ContestModule
{
    /// <summary>
    /// Extensions for contest context.
    /// </summary>
    public static class ContestContextExtensions
    {
        /// <summary>
        /// Validate whether this user has permission to the contest jury part.
        /// </summary>
        /// <param name="context">The contest context.</param>
        /// <param name="user">The user claims principal.</param>
        /// <returns>The task for validation, <c>true</c> for jury and <c>false</c> for non-jury.</returns>
        public static async Task<bool> ValidateAsync(this IContestContext context, ClaimsPrincipal user)
        {
            if (user.IsInRole("Administrator")) return true;
            if (!int.TryParse(user.GetUserId(), out var uid))
                return false;
            var jury = await context.FetchJuryAsync();
            return jury.ContainsKey(uid);
        }

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
        /// Fetch the contest problem by ID.
        /// </summary>
        /// <param name="context">The contest context.</param>
        /// <param name="probid">The problem ID.</param>
        /// <returns>The task for fetching problems.</returns>
        public static async Task<ProblemModel?> FindProblemAsync(this IContestContext context, int probid)
        {
            var probs = await context.FetchProblemsAsync();
            return probs.Find(probid);
        }

        /// <summary>
        /// Fetch the contest problem by short name.
        /// </summary>
        /// <param name="context">The contest context.</param>
        /// <param name="shortName">The problem short name.</param>
        /// <returns>The task for fetching problems.</returns>
        public static async Task<ProblemModel?> FindProblemAsync(this IContestContext context, string shortName)
        {
            var probs = await context.FetchProblemsAsync();
            return probs.Find(shortName);
        }

        /// <summary>
        /// Gets the single board view.
        /// </summary>
        /// <param name="context">The contest context.</param>
        /// <param name="teamid">The team ID.</param>
        /// <returns>The single scoreboard.</returns>
        public static async Task<SingleBoardViewModel?> SingleBoardAsync(
            this IContestContext context,
            int teamid)
        {
            var scb = await context.FetchScoreboardAsync();
            var bq = scb.Data.GetValueOrDefault(teamid);
            if (bq == null) return null;
            var cats = await context.FetchCategoriesAsync();
            var affs = await context.FetchAffiliationsAsync();

            return new SingleBoardViewModel
            {
                QueryInfo = bq,
                Contest = context.Contest,
                Problems = await context.FetchProblemsAsync(),
                Affiliation = affs[bq.AffiliationId],
                Category = cats[bq.CategoryId],
            };
        }
    }
}
