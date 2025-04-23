namespace Skyline.DataMiner.CICD.Tools.DmUpgradeStorage.Lib
{
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Static class for default debug logging messages.
    /// </summary>
    public static partial class DebugLog
    {
        /// <summary>
        /// Indicate the start of the method.
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="callerMemberName">Optional calling member name.</param>
        [LoggerMessage(LogLevel.Debug, "Starting {callerMemberName}...")]
        public static partial void Start(ILogger logger, [CallerMemberName] string callerMemberName = "");

        /// <summary>
        /// Indicate the end of the method.
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="callerMemberName">Optional calling member name.</param>
        [LoggerMessage(LogLevel.Debug, "Finished {callerMemberName}.")]
        public static partial void End(ILogger logger, [CallerMemberName] string callerMemberName = "");

        /// <summary>
        /// Indicate the end of the method.
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="sw">The stopwatch.</param>
        /// <param name="callerMemberName">Optional calling member name.</param>
        [LoggerMessage(LogLevel.Debug, "Finished {callerMemberName} in {sw}.")]
        public static partial void End(ILogger logger, Stopwatch sw, [CallerMemberName] string callerMemberName = "");
    }
}