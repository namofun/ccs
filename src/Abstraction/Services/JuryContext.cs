﻿using Ccs.Entities;
using Ccs.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs.Services
{
    /// <summary>
    /// Provides contract for jury.
    /// </summary>
    public interface IJuryContext : IContestContext
    {
        /// <summary>
        /// Get the status of judge queue.
        /// </summary>
        /// <returns>The list of judge queues.</returns>
        Task<Polygon.Models.ServerStatus> GetJudgeQueueAsync();

        /// <summary>
        /// Asynchronously update the contest.
        /// </summary>
        /// <param name="updateExpression">The update expression.</param>
        /// <returns>The task for updating the contest.</returns>
        Task<ContestWrapper> UpdateContestAsync(Expression<Func<Contest, Contest>> updateExpression);

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
        /// <param name="source">The original source code.</param>
        /// <returns>The task for saving readme.</returns>
        Task SetReadmeAsync(string source);

        /// <summary>
        /// Get the ajax update overview.
        /// </summary>
        /// <returns>The object for update results.</returns>
        Task<object> GetUpdatesAsync();

        /// <summary>
        /// Get the auditlogs.
        /// </summary>
        /// <param name="page">The page to show.</param>
        /// <param name="pageCount">The count of pages to show.</param>
        /// <returns>The task with auditlogs.</returns>
        Task<IPagedList<SatelliteSite.Entities.Auditlog>> ViewLogsAsync(int page, int pageCount);
    }
}