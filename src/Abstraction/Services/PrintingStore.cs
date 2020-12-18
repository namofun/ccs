using Ccs.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    /// <summary>
    /// The storage interface for <see cref="Printing"/>.
    /// </summary>
    /// <remarks>Note that all store interfaces shouldn't cache the result.</remarks>
    public interface IPrintingStore
    {
        /// <summary>
        /// Create an instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The task for creating entity.</returns>
        Task<Printing> CreateAsync(Printing entity);

        /// <summary>
        /// Set the state of contest printing.
        /// </summary>
        /// <param name="printing">The printing entity.</param>
        /// <param name="done">The state.</param>
        /// <returns>The task for setting state.</returns>
        Task<bool> SetStateAsync(Printing printing, bool? done);

        /// <summary>
        /// Find the printing entity.
        /// </summary>
        /// <param name="id">The printing ID.</param>
        /// <returns>The task for fetching entity.</returns>
        Task<Printing?> FindAsync(int id);

        /// <summary>
        /// Find the first printing entity satisfying the condition.
        /// </summary>
        /// <param name="condition">The condition expression.</param>
        /// <returns>The task for fetching entity.</returns>
        Task<Printing?> FirstAsync(Expression<Func<Printing, bool>> condition);

        /*
        [Obsolete("This is going to be re-designed.", true)]
        Task<List<T>> ListAsync<T>(int takeCount, int page,
            Expression<Func<Printing, object, Team, T>> expression,
            Expression<Func<Printing, bool>>? predicate = null);
        */
    }
}
