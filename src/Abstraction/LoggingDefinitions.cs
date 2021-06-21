using Ccs.Models;
using Microsoft.Extensions.Logging;
using System;

namespace Ccs
{
    /// <summary>
    /// Logging definitions for <see cref="ILogger"/>.
    /// </summary>
    public static class LoggingDefinitions
    {
        /// <summary>
        /// Warning: The call <paramref name="funcName"/> to current <paramref name="contest"/> is not proper.
        /// </summary>
        public static void ImproperCall(this ILogger logger, string funcName, IContestInformation contest)
            => _improperCall(logger, funcName, contest.Feature, null);

        #region Definitions

        private static readonly Action<ILogger, string, int, Exception?> _improperCall =
            LoggerMessage.Define<string, int>(
                logLevel: LogLevel.Warning,
                eventId: new EventId(10714),
                formatString: "The call {funcName} to current contest feature set {kind} is not proper.");

        #endregion
    }
}
