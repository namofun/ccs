using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xylab.Polygon.Models;

namespace Xylab.Contesting.Services
{
    /// <summary>
    /// Extension methods for ccs related things.
    /// </summary>
    public static class ServicingExtensions
    {
        /// <summary>
        /// Converts the store as a <see cref="IContestQueryableStore"/>.
        /// </summary>
        /// <param name="store">The <see cref="IContestRepository"/>.</param>
        /// <returns>The <see cref="IContestQueryableStore"/>.</returns>
        public static IContestQueryableStore GetQueryableStore(this IContestRepository store)
            => store as IContestQueryableStore
                ?? throw new InvalidOperationException("This store is not a IQueryable store.");

        /// <summary>
        /// Converts the store as a <see cref="IContestQueryableStore"/>.
        /// </summary>
        /// <param name="store">The <see cref="IContestContext"/>.</param>
        /// <returns>The <see cref="IContestQueryableStore"/>.</returns>
        public static IContestQueryableStore GetQueryableStore(this IContestContext store)
            => store as IContestQueryableStore
                ?? throw new InvalidOperationException("This store is not a IQueryable store.");

        /// <summary>
        /// Applies the team name to the solution list.
        /// </summary>
        /// <param name="context">The <see cref="IContestContext"/>.</param>
        /// <param name="solutions">The list of <see cref="Solution"/>s.</param>
        /// <returns>The task for applying.</returns>
        public static async Task ApplyTeamNamesAsync(this IContestContext context, IReadOnlyList<Solution> solutions)
        {
            var tn = await context.GetTeamNamesAsync();
            foreach (var solu in solutions)
            {
                solu.AuthorName = tn.TryGetValue(solu.TeamId, out var an) ? an : string.Empty;
            }
        }
    }
}
