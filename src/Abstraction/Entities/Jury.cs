using System.Collections.Generic;

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

        /// <summary>
        /// The jury level
        /// </summary>
        public JuryLevel Level { get; set; }

        /// <summary>
        /// Valid jury levels
        /// </summary>
        public static readonly IReadOnlyList<JuryLevel> ValidValues = new[]
        {
            JuryLevel.BalloonRunner,
            JuryLevel.Jury,
            JuryLevel.Administrator
        };
    }

    /// <summary>
    /// The enum for levels of jury.
    /// </summary>
    public enum JuryLevel
    {
        /// <summary>
        /// The level that only can send balloons and prints
        /// </summary>
        BalloonRunner = 0,

        /// <summary>
        /// The level that manipulates many things
        /// </summary>
        Jury = 10,

        /// <summary>
        /// The level that can add more jury, refresh cache, or dangerous operations
        /// </summary>
        Administrator = 20,
    }
}
