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
        /// The navigation to team
        /// </summary>
        public Team Team { get; set; }

        /// <summary>
        /// The user ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Whether the linked user is temporary account
        /// </summary>
        public bool Temporary { get; set; }

        /// <summary>
        /// The rating delta for Codeforces Rounds Rating
        /// </summary>
        public int? RatingDelta { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Instantiate an entity for <see cref="Member"/>.
        /// </summary>
        public Member()
        {
        }
#pragma warning restore CS8618
    }
}
