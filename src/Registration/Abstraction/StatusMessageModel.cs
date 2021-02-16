using Ccs.Entities;

namespace Ccs.Registration
{
    /// <summary>
    /// The model class for status message.
    /// </summary>
    public class StatusMessageModel
    {
        /// <summary>
        /// The success flag
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// The status message
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// The created team ID.
        /// </summary>
        public int? TeamId { get; }

        /// <summary>
        /// Creates a model for success.
        /// </summary>
        /// <param name="team">The created team.</param>
        /// <returns>The created model.</returns>
        public static StatusMessageModel Succeed(Team team)
        {
            return new StatusMessageModel(true, "Register succeeded.", team.TeamId);
        }

        /// <summary>
        /// Creates a model for failure.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>The created model.</returns>
        public static StatusMessageModel Fail(string message)
        {
            return new StatusMessageModel(false, message);
        }

        /// <summary>
        /// Initialize the <see cref="StatusMessageModel"/>.
        /// </summary>
        /// <param name="successful">Whether succeeded.</param>
        /// <param name="content">The status message.</param>
        /// <param name="teamid">The team ID.</param>
        private StatusMessageModel(bool successful, string content, int? teamid = null)
        {
            Succeeded = successful;
            Content = content;
            TeamId = teamid;
        }
    }
}
