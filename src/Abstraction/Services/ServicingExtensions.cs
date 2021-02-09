using System;

namespace Ccs.Services
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
    }
}
