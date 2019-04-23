namespace LightInject
{
    using System;

   /// <summary>
    /// Extends the log delegate to simplify creating log entries.
    /// </summary>
    public static class LogExtensions
    {
        /// <summary>
        /// Logs a new entry with the <see cref="LogLevel.Info"/> level.
        /// </summary>
        /// <param name="logAction">The log delegate.</param>
        /// <param name="message">The message to be logged.</param>
        public static void Info(this Action<LogEntry> logAction, string message)
        {
            logAction(new LogEntry(LogLevel.Info, message));
        }

        /// <summary>
        /// Logs a new entry with the <see cref="LogLevel.Warning"/> level.
        /// </summary>
        /// <param name="logAction">The log delegate.</param>
        /// <param name="message">The message to be logged.</param>
        public static void Warning(this Action<LogEntry> logAction, string message)
        {
            logAction(new LogEntry(LogLevel.Warning, message));
        }
    }
}