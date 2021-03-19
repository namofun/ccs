﻿using Ccs.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ccs.Services
{
    /// <summary>
    /// The conventional interface for rating updater.
    /// </summary>
    public interface IRatingUpdater
    {
        /// <summary>
        /// Gets whether rating update is supported.
        /// </summary>
        bool SupportRatingUpdate { get; }

        /// <summary>
        /// Rolls back the contest ratings.
        /// </summary>
        /// <param name="contest">The contest information.</param>
        /// <returns>The task for rolling back the rating changes.</returns>
        Task RollbackAsync(IContestInformation contest);

        /// <summary>
        /// Applies the contest ratings.
        /// </summary>
        /// <param name="contest">The contest information.</param>
        /// <returns>The task for applying the rating changes.</returns>
        Task ApplyAsync(IContestInformation contest);
    }

    /// <summary>
    /// The basic implementation for real rating updaters.
    /// </summary>
    public abstract class RatingUpdaterBase : IRatingUpdater
    {
        protected readonly IRatingCalculator _ratingCalculator;

        /// <inheritdoc />
        public bool SupportRatingUpdate => true;

        /// <summary>
        /// Initialize the <see cref="IRatingUpdater"/>.
        /// </summary>
        /// <param name="ratingCalculator">The rating calculator.</param>
        protected RatingUpdaterBase(IRatingCalculator ratingCalculator)
        {
            _ratingCalculator = ratingCalculator;
        }

        /// <summary>
        /// Gets the previous ratings for participants.
        /// </summary>
        /// <param name="contest">The contest information.</param>
        /// <returns>The task for getting the rating.</returns>
        protected abstract Task<List<ParticipantRating>> GetPreviousRatingsAsync(IContestInformation contest);

        /// <summary>
        /// Applies the rating changes for participants, both on the user entity and team member entity.
        /// </summary>
        /// <param name="contest">The contest information.</param>
        /// <param name="results">The rating results.</param>
        /// <returns>The task for setting the rating.</returns>
        protected abstract Task ApplyRatingChangesAsync(IContestInformation contest, IReadOnlyList<ParticipantRating> results);

        /// <summary>
        /// Validates whether the contest rating has been applied.
        /// </summary>
        /// <param name="contest">The contest information.</param>
        /// <param name="shouldHaveBeen">Whether the contest should have been applied or not.</param>
        /// <returns>The task for validating. If validation is failed, throws an exception.</returns>
        /// <exception cref="InvalidOperationException" />
        protected virtual Task ValidateAsync(IContestInformation contest, bool shouldHaveBeen)
        {
            if (contest.RankingStrategy != CcsDefaults.RuleCodeforces)
            {
                throw new InvalidOperationException("The ranking strategy must be codeforces.");
            }

            if (contest.GetState() != Entities.ContestState.Finalized)
            {
                throw new InvalidOperationException("The contest hasn't been finalized.");
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected abstract Task RollbackRatingChangesAsync(IContestInformation contest);

        /// <inheritdoc />
        public async Task ApplyAsync(IContestInformation contest)
        {
            await ValidateAsync(contest, shouldHaveBeen: false);
            var participants = await GetPreviousRatingsAsync(contest);
            _ratingCalculator.ComputeRatingChanges(participants);
            await ApplyRatingChangesAsync(contest, participants);
        }

        /// <inheritdoc />
        public async Task RollbackAsync(IContestInformation contest)
        {
            await ValidateAsync(contest, shouldHaveBeen: true);
            await RollbackRatingChangesAsync(contest);
        }
    }
}