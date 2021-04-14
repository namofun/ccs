namespace Ccs.Entities
{
    /// <summary>
    /// The entity class for team members.
    /// </summary>
    public class Member
    {
        /// <summary>
        /// The contest ID
        /// </summary>
        public int ContestId { get; set; }

        /// <summary>
        /// The team ID
        /// </summary>
        public int TeamId { get; set; }

        /// <summary>
        /// The user ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Whether the linked user is temporary account
        /// </summary>
        public bool Temporary { get; set; }

        /// <summary>
        /// The previous rating information
        /// </summary>
        public int? PreviousRating { get; set; }

        /// <summary>
        /// The rating delta for Codeforces Rounds Rating
        /// </summary>
        public int? RatingDelta { get; set; }

        /// <summary>
        /// The last login IP
        /// </summary>
        public string? LastLoginIp { get; set; }
    }
}
