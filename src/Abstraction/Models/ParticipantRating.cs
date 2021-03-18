namespace Ccs.Models
{
    /// <summary>
    /// The model class for contest participant.
    /// </summary>
    public class ParticipantRating
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int UserId { get; }

        /// <summary>
        /// Current user rating
        /// </summary>
        public int? UserRating { get; }

        /// <summary>
        /// Final rank in contest
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Points in contest
        /// </summary>
        public int Points { get; }

        /// <summary>
        /// Whether participant is first to come
        /// </summary>
        public bool NewComer => !UserRating.HasValue;

        /// <summary>
        /// Previous rating for participant
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// Needed rating to achieve this rank
        /// </summary>
        public int NeedRating { get; set; }

        /// <summary>
        /// Seed for calculating (Elo win probability)
        /// </summary>
        public double Seed { get; set; }
        
        /// <summary>
        /// Delta change of rating in this round
        /// </summary>
        public int Delta { get; set; }

        /// <summary>
        /// Initialize the <see cref="ParticipantRating"/>.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="previousRating">The previous rating for user.</param>
        /// <param name="points">The codeforces mode points.</param>
        public ParticipantRating(int userId, int? previousRating, int points)
        {
            UserId = userId;
            UserRating = previousRating;
            Points = points;
        }
    }
}
