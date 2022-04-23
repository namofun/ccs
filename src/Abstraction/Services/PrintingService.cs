using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xylab.Contesting.Entities;
using Xylab.Contesting.Models;

namespace Xylab.Contesting.Services
{
    /// <summary>
    /// The storage interface for <see cref="Printing"/>.
    /// </summary>
    public interface IPrintingService
    {
        /// <summary>
        /// Creates an instance of printing.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The task for creating entity.</returns>
        Task<Printing> CreateAsync(Printing entity);

        /// <summary>
        /// Sets the state of contest printing.
        /// </summary>
        /// <param name="printing">The printing entity.</param>
        /// <param name="done">The state.</param>
        /// <returns>The task for setting state.</returns>
        Task<bool> SetStateAsync(Printing printing, bool? done);

        /// <summary>
        /// Finds the printing entity.
        /// </summary>
        /// <param name="id">The printing ID.</param>
        /// <param name="full">Whether to include source code.</param>
        /// <returns>The task for fetching entity.</returns>
        Task<Printing?> FindAsync(int id, bool full = false);

        /// <summary>
        /// Finds the first printing entity satisfying the condition.
        /// </summary>
        /// <param name="condition">The condition expression.</param>
        /// <returns>The task for fetching entity.</returns>
        /// <remarks>This implementation doesn't need to be thread safe.</remarks>
        Task<Printing?> FirstAsync(Expression<Func<Printing, bool>> condition);

        /// <summary>
        /// Lists the printing tasks for jury page.
        /// </summary>
        /// <param name="contestId">The contest ID.</param>
        /// <param name="limit">The count per page.</param>
        /// <returns>The task for fetching models.</returns>
        Task<List<PrintingTask>> ListAsync(int contestId, int limit);
    }
}
