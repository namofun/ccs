namespace Ccs.Scoreboard.Ratings
{
    /// <summary>
    /// The model class for contest participant.
    /// </summary>
    public class Participant
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Current user rating
        /// </summary>
        public int? UserRating { get; set; }

        /// <summary>
        /// Final rank in contest
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Points in contest
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Whether participant is first to come
        /// </summary>
        public bool NewComer => !UserRating.HasValue;

        /// <summary>
        /// Previous rating for participant
        /// </summary>
        public int Rating => UserRating ?? RatingCalculator.INITIAL_RATING;

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
    }
}
