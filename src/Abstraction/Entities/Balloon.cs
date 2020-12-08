using Polygon.Entities;

namespace Ccs.Entities
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
        /// The navigation to submission
        /// </summary>
        public Submission Submission { get; set; }

        /// <summary>
        /// The submission ID
        /// </summary>
        public int SubmissionId { get; set; }

        /// <summary>
        /// Whether the balloon has been handed out
        /// </summary>
        public bool Done { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Instantiate an entity for <see cref="Balloon"/>.
        /// </summary>
        public Balloon()
        {
        }
#pragma warning restore CS8618
    }
}
