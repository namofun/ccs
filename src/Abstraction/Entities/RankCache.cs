namespace Ccs.Entities
{
    /// <summary>
    /// The entity class for rank cache.
    /// </summary>
    public class RankCache
    {
        /// <summary>
        /// Represents an empty item
        /// </summary>
        public static readonly RankCache Empty = new RankCache();

        /// <summary>
        /// The contest ID
        /// </summary>
        public int ContestId { get; set; }

        /// <summary>
        /// The team ID
        /// </summary>
        public int TeamId { get; set; }

        /// <summary>
        /// The total points (Restricted)
        /// </summary>
        public int PointsRestricted { get; set; }

        /// <summary>
        /// The penalty time (Restricted)
        /// </summary>
        public int TotalTimeRestricted { get; set; }

        /// <summary>
        /// The total points (Public)
        /// </summary>
        public int PointsPublic { get; set; }

        /// <summary>
        /// The penalty time (Public)
        /// </summary>
        public int TotalTimePublic { get; set; }
    }
}
