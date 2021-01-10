﻿using Ccs.Entities;
using Ccs.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ccs.Services
{
    /// <summary>
    /// The storage interface for <see cref="Contest"/>.
    /// </summary>
    /// <remarks>Note that the implementation for this interface shouldn't cache the result.</remarks>
    public interface IContestRepository
    {
        /// <summary>
        /// Create an instance of entity.
        /// </summary>
        /// <param name="kind">The contest kind.</param>
        /// <param name="user">The creation user.</param>
        /// <returns>The task for creating entity.</returns>
        Task<Contest> CreateAndAssignAsync(int kind, ClaimsPrincipal user);

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
    }
}