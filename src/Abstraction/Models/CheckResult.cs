namespace Xylab.Contesting.Models
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
        protected CheckResult(bool success, string? message)
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

    /// <summary>
    /// The check result of things like availability or execution result.
    /// </summary>
    public class CheckResult<T> : CheckResult where T : class
    {
        /// <summary>
        /// Gets the execution result.
        /// </summary>
        public T? Result { get; }

        /// <summary>
        /// Instantiate a check result.
        /// </summary>
        /// <param name="success">The result.</param>
        /// <param name="message">The message.</param>
        /// <param name="result">The result.</param>
        private CheckResult(bool success, string? message, T? result)
            : base(success, message) => Result = result;

        /// <summary>
        /// Instantiate a succeeded result.
        /// </summary>
        /// <param name="message">The message.</param>
        public static new CheckResult<T> Succeed(string? message = null)
            => new CheckResult<T>(true, message, null);

        /// <summary>
        /// Instantiate a succeeded result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="message">The message.</param>
        public static CheckResult<T> Succeed(T result, string? message = null)
            => new CheckResult<T>(true, message, result);

        /// <summary>
        /// Instantiate a failed result.
        /// </summary>
        /// <param name="message">The message.</param>
        public static new CheckResult<T> Fail(string? message = null)
            => new CheckResult<T>(false, message, null);
    }
}
