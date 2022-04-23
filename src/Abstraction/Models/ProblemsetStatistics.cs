namespace Xylab.Contesting.Models
{
    /// <summary>
    /// The model class for problemset submission statistics.
    /// </summary>
    public class ProblemsetStatistics
    {
        /// <summary>
        /// The problem ID
        /// </summary>
        public string ProblemId { get; set; } = string.Empty;

        /// <summary>
        /// The count of total submissions
        /// </summary>
        public int TotalSubmission { get; set; }

        /// <summary>
        /// The count of accepted submissions
        /// </summary>
        public int AcceptedSubmission { get; set; }
    }
}
