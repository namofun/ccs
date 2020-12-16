namespace Ccs.Entities
{
    /// <summary>
    /// The entity class for contest jury.
    /// </summary>
    public class Jury
    {
        /// <summary>
        /// The contest ID
        /// </summary>
        public int ContestId { get; set; }

        /// <summary>
        /// The jury user ID
        /// </summary>
        public int UserId { get; set; }
    }
}
