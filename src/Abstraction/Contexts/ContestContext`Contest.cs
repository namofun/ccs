using Ccs.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs
{
    public partial interface IContestContext
    {
        /// <summary>
        /// Asynchronously update the contest.
        /// </summary>
        /// <param name="updateExpression">The update expression.</param>
        /// <returns>The task for updating the contest.</returns>
        Task<Contest> UpdateContestAsync(Expression<Func<Contest, Contest>> updateExpression);

        /// <summary>
        /// Fetch the ID of the latest event.
        /// </summary>
        /// <returns>The task for ID of event.</returns>
        Task<int> MaxEventIdAsync();

        /// <summary>
        /// Fetch the event for a contest after some ID.
        /// </summary>
        /// <param name="type">The endpoint type.</param>
        /// <param name="after">The first event ID.</param>
        /// <returns>The task for fetching new events.</returns>
        Task<List<Event>> FetchEventAsync(string[]? type = null, int after = 0);

        /// <summary>
        /// Get the jury list.
        /// </summary>
        /// <returns>The task for fetching jury list.</returns>
        /// <remarks>Implementation should take a <see cref="HashSet{T}"/> as resulting type.</remarks>
        Task<Dictionary<int, string>> FetchJuryAsync();

        /// <summary>
        /// Assign jury user to contest.
        /// </summary>
        /// <param name="user">The jury user.</param>
        /// <returns>The task for assigning jury.</returns>
        Task AssignJuryAsync(IUser user);

        /// <summary>
        /// Unassign jury user to contest.
        /// </summary>
        /// <param name="user">The jury user.</param>
        /// <returns>The task for assigning jury.</returns>
        Task UnassignJuryAsync(IUser user);

        /// <summary>
        /// Get the readme content.
        /// </summary>
        /// <param name="source">The source code.</param>
        /// <returns>The task for reading me.</returns>
        Task<string> GetReadmeAsync(bool source = false);

        /// <summary>
        /// Get the readme content.
        /// </summary>
        /// <param name="source">The original source code.</param>
        /// <returns>The task for saving readme.</returns>
        Task SetReadmeAsync(string source);
    }
}
