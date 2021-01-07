﻿using Ccs.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ccs
{
    public partial interface IContestContext
    {
        /// <summary>
        /// List the clarifications via predicate.
        /// </summary>
        /// <param name="predicate">The condition to match.</param>
        /// <returns>The task for listing clarifications.</returns>
        Task<List<Clarification>> ListClarificationsAsync(Expression<Func<Clarification, bool>> predicate);

        /// <summary>
        /// Find the clarification via ID.
        /// </summary>
        /// <param name="id">The clarification ID.</param>
        /// <returns>The task for fetching clarification.</returns>
        Task<Clarification> FindClarificationAsync(int id);

        /// <summary>
        /// Send a clarification.
        /// </summary>
        /// <param name="clar">The new clarification to create.</param>
        /// <param name="replyTo">The clarification to reply to.</param>
        /// <returns>The task for creating clarification.</returns>
        Task<Clarification> ClarifyAsync(Clarification clar, Clarification? replyTo = null);
    }
}
