namespace Xylab.Contesting.Entities
{
    /// <summary>
    /// The entity class for score cache.
    /// </summary>
    public class ScoreCache
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
        /// The problem ID
        /// </summary>
        public int ProblemId { get; set; }

        /// <summary>
        /// The submission count (Restricted)
        /// </summary>
        public int SubmissionRestricted { get; set; }

        /// <summary>
        /// The pending count (Restricted)
        /// </summary>
        public int PendingRestricted { get; set; }

        /// <summary>
        /// The solve time (Restricted)
        /// </summary>
        public double? SolveTimeRestricted { get; set; }

        /// <summary>
        /// The current score (Restricted)
        /// </summary>
        public int? ScoreRestricted { get; set; }

        /// <summary>
        /// Whether this is correct (Restricted)
        /// </summary>
        public bool IsCorrectRestricted { get; set; }

        /// <summary>
        /// The submission count (Public)
        /// </summary>
        public int SubmissionPublic { get; set; }

        /// <summary>
        /// The pending count (Public)
        /// </summary>
        public int PendingPublic { get; set; }

        /// <summary>
        /// The solve time (Public)
        /// </summary>
        public double? SolveTimePublic { get; set; }

        /// <summary>
        /// The current score (Public)
        /// </summary>
        public int? ScorePublic { get; set; }

        /// <summary>
        /// Whether this is correct (Public)
        /// </summary>
        public bool IsCorrectPublic { get; set; }

        /// <summary>
        /// Whether this is the first one to solve the problem in this sort order
        /// </summary>
        public bool FirstToSolve { get; set; }
    }
}
