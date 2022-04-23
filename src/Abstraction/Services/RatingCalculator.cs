using System.Collections.Generic;
using Xylab.Contesting.Entities;
using Xylab.Contesting.Models;

namespace Xylab.Contesting.Services
{
    /// <summary>
    /// The conventional interface for a rating calculator.
    /// </summary>
    public interface IRatingCalculator
    {
        /// <summary>
        /// The initial rating value
        /// </summary>
        int InitialRating { get; }

        /// <summary>
        /// Calculates the normal rating for participants.
        /// </summary>
        /// <param name="ratingChanges">Rating changes represented by <see cref="Member"/> entity.</param>
        /// <returns>The latest member rating.</returns>
        int? AggregateRating(IReadOnlyList<Member> ratingChanges);

        /// <summary>
        /// Calculates the maximum rating for participants.
        /// </summary>
        /// <param name="ratingChanges">Rating changes represented by <see cref="Member"/> entity.</param>
        /// <returns>The maximum member rating.</returns>
        int? GetMaxRating(IReadOnlyList<Member> ratingChanges);

        /// <summary>
        /// Computes the rating changes.
        /// </summary>
        /// <param name="contestants">The contestant information.</param>
        void ComputeRatingChanges(List<ParticipantRating> contestants);
    }
}
