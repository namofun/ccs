using Ccs.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    /// <summary>
    /// The storage interface for <see cref="Clarification"/>.
    /// </summary>
    /// <remarks>Note that all store interfaces shouldn't cache the result.</remarks>
    public interface IClarificationStore
    {
        /// <summary>
        /// Find the clarification via ID.
        /// </summary>
        /// <param name="contest">The contest.</param>
        /// <param name="id">The clarification ID.</param>
        /// <returns>The task for fetching clarification.</returns>
        Task<Clarification> FindAsync(Contest contest, int id);

        /// <summary>
        /// List the clarifications via predicate.
        /// </summary>
        /// <param name="contest">The contest ID.</param>
        /// <param name="predicate">The condition to match.</param>
        /// <returns>The task for listing clarifications.</returns>
        Task<List<Clarification>> ListAsync(
            Contest contest,
            Expression<Func<Clarification, bool>>? predicate = null);

        /// <summary>
        /// Count the unanswered clarifications.
        /// </summary>
        /// <param name="contest">The contest.</param>
        /// <returns>The task for counting.</returns>
        Task<int> CountUnansweredAsync(Contest contest);
        
        /// <summary>
        /// Send a clarification.
        /// </summary>
        /// <param name="clar">The new clarification to create.</param>
        /// <param name="replyTo">The clarification to reply to.</param>
        /// <returns>The task for creating clarification.</returns>
        Task<Clarification> SendAsync(Clarification clar, Clarification? replyTo = null);

        /// <summary>
        /// Mark the clarification as <paramref name="answered"/> directly.
        /// </summary>
        /// <param name="contest">The contest.</param>
        /// <param name="id">The clarification ID.</param>
        /// <param name="answered">Whether this clarification is answered.</param>
        /// <returns>The task for checking results.</returns>
        Task<bool> SetAnsweredAsync(Contest contest, int id, bool answered);

        /// <summary>
        /// Claim or disclaim the clarification via jury directly.
        /// </summary>
        /// <param name="contest">The contest.</param>
        /// <param name="id">The clarification ID.</param>
        /// <param name="jury">The jury name.</param>
        /// <param name="claim">Claim or disclaim.</param>
        /// <returns>The task for claiming.</returns>
        Task<bool> ClaimAsync(Contest contest, int id, string jury, bool claim);
    }
}
