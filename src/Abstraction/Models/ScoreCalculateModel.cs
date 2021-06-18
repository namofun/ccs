using Polygon.Entities;
using System;

namespace Ccs.Models
{
    /// <summary>
    /// The core model for recalculating scoreboard.
    /// </summary>
    public class ScoreCalculateModel
    {
        /// <summary>
        /// The submission ID
        /// </summary>
        public int SubmissionId { get; set; }

        /// <summary>
        /// The team ID
        /// </summary>
        public int TeamId { get; set; }

        /// <summary>
        /// The problem ID
        /// </summary>
        public int ProblemId { get; set; }

        /// <summary>
        /// The submission time
        /// </summary>
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// The judging result
        /// </summary>
        public Verdict Status { get; set; }

        /// <summary>
        /// The rejudging ID
        /// </summary>
        public int? RejudgingId { get; set; }

        /// <summary>
        /// The total score
        /// </summary>
        public int TotalScore { get; set; }

        /// <summary>
        /// The sort order
        /// </summary>
        public int SortOrder { get; set; }
    }
}
