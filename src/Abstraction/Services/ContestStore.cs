using Ccs.Entities;
using Ccs.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    /// <summary>
    /// The storage interface for <see cref="Contest"/>.
    /// </summary>
    public interface IContestStore
    {
        /// <summary>
        /// Create an instance of entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The task for creating entity.</returns>
        Task<Contest> CreateAsync(Contest entity);

        /// <summary>
        /// Update the instance of entity.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="expression">The updating expression.</param>
        /// <returns>The task for updating.</returns>
        Task UpdateAsync(int cid, Expression<Func<Contest, Contest>> expression);

        /// <summary>
        /// Find the instance of entity.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <returns>The task for fetching entity.</returns>
        Task<Contest> FindAsync(int cid);

        /// <summary>
        /// List the contests with user and kind limitation.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="kind">The contest kind.</param>
        /// <param name="page">The current page.</param>
        /// <param name="limit">The count per page.</param>
        /// <returns>The task for fetching paged lists.</returns>
        Task<IPagedList<ContestListModel>> ListAsync(int userId, int kind, int page = 1, int limit = 100);

        /// <summary>
        /// List the contests with original entity.
        /// </summary>
        /// <param name="page">The current page.</param>
        /// <param name="limit">The count per page.</param>
        /// <returns>The task for fetching paged lists.</returns>
        Task<IPagedList<ContestListModel>> ListAsync(int page = 1, int limit = 100);

        /// <summary>
        /// Fetch the event for a contest after some ID.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <param name="after">The first event ID.</param>
        /// <returns>The task for fetching new events.</returns>
        Task<List<Event>> FetchEventAsync(int cid, int after = 0);

        /// <summary>
        /// Fetch the ID of the latest event.
        /// </summary>
        /// <param name="cid">The contest ID.</param>
        /// <returns>The task for ID of event.</returns>
        Task<int> MaxEventIdAsync(int cid);

        /// <summary>
        /// Emit an event.
        /// </summary>
        /// <param name="event">The new event entity.</param>
        /// <returns>The task for emitting.</returns>
        Task EmitAsync(Event @event);
    }
}
