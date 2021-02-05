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
        /// Initialize the <see cref="StatusMessageModel"/>.
        /// </summary>
        /// <param name="successful">Whether succeeded.</param>
        /// <param name="content">The status message.</param>
        public StatusMessageModel(bool successful, string content)
        {
            Succeeded = successful;
            Content = content;
        }
    }
}
