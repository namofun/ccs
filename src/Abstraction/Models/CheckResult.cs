namespace Ccs.Models
{
    /// <summary>
    /// The check result of things like availability.
    /// </summary>
    public class CheckResult
    {
        /// <summary>
        /// Gets whether the check succeeded.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the check message.
        /// </summary>
        public string? Message { get; }

        /// <summary>
        /// Instantiate a check result.
        /// </summary>
        /// <param name="success">The result.</param>
        /// <param name="message">The message.</param>
        private CheckResult(bool success, string? message)
            => (Success, Message) = (success, message);

        /// <summary>
        /// Instantiate a succeeded result.
        /// </summary>
        /// <param name="message">The message.</param>
        public static CheckResult Succeed(string? message = null)
            => new CheckResult(true, message);

        /// <summary>
        /// Instantiate a failed result.
        /// </summary>
        /// <param name="message">The message.</param>
        public static CheckResult Fail(string? message = null)
            => new CheckResult(false, message);
    }
}
