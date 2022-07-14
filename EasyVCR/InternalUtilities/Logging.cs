using System;
using Microsoft.Extensions.Logging;

namespace EasyVCR.InternalUtilities
{
    /// <summary>
    ///     A logger that, if there's no proper logger, writes to the console instead.
    /// </summary>
    internal class ConsoleFallbackLogger
    {
        private readonly ILogger? _innerLogger;

        private readonly string _name;


        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="logger"><see cref="ILogger" /> logger to optionally use internally.</param>
        /// <param name="name">Name of application adding entries.</param>
        internal ConsoleFallbackLogger(ILogger? logger, string name)
        {
            _name = name;
            _innerLogger = logger;
        }

        /// <summary>
        ///     Logs an error message to the logger or console.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="args">Arguments for message formatting.</param>
        internal void Error(string message, params object?[] args)
        {
            Log(
                (msg, a) => _innerLogger?.LogError(msg, a),
                (msg, a) => Console.Error.WriteLine(msg, a),
                message,
                args);
        }

        /// <summary>
        ///     Logs a warning message to the logger or console.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="args">Arguments for message formatting.</param>
        internal void Warning(string message, params object?[] args)
        {
            Log(
                (msg, a) => _innerLogger?.LogWarning(msg, a),
                Console.WriteLine,
                message,
                args);
        }

        /// <summary>
        ///     Log a message using the logFunc if there's a logger, otherwise using the fallback action.
        /// </summary>
        /// <param name="logFunc">Function to execute if a logger exists.</param>
        /// <param name="fallbackAction">Function to execute if a logger does not exist.</param>
        /// <param name="message">Message to be passed to function.</param>
        /// <param name="args">Message arguments to be passed to function.</param>
        private void Log(Action<string, object?[]> logFunc, Action<string, object?[]> fallbackAction, string message, params object?[] args)
        {
            if (_innerLogger != null)
            {
                logFunc(_name, args);
            }
            else
            {
                fallbackAction(MakeMessage(message), args);
            }
        }

        private string MakeMessage(string message) => $"{_name}: {message}";
    }
}
