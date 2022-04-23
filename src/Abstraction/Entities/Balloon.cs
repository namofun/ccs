namespace Xylab.Contesting.Entities
{
    /// <summary>
    /// The entity class for balloons in contests.
    /// </summary>
    public class Balloon
    {
        /// <summary>
        /// The balloon ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The submission ID
        /// </summary>
        public int SubmissionId { get; set; }

        /// <summary>
        /// Whether the balloon has been handed out
        /// </summary>
        public bool Done { get; set; }
    }
}
