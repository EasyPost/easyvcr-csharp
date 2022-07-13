using System;

namespace EasyVCR
{
    /// <summary>
    ///     Enums representing different actions to take when a recording is expired.
    /// </summary>
    public enum ExpirationActions
    {
        /// <summary>
        ///     Warn that the recorded interaction is expired, but proceed as normal.
        /// </summary>
        Warn,
        /// <summary>
        ///     Throw an exception that the recorded interaction is expired.
        /// </summary>
        ThrowException,
        /// <summary>
        ///     Automatically re-record the recorded interaction. This cannot be used with <see cref="Mode.Replay" />.
        /// </summary>
        RecordAgain
    }

    internal static class ExpirationActionExtensions
    {
        internal static void CheckCompatibleSettings(ExpirationActions action, Mode mode)
        {
            if (action == ExpirationActions.RecordAgain && mode == Mode.Replay)
            {
                throw new VCRException("Cannot use the RecordAgain expiration action in combination with Replay mode.");
            }
        }
    }
}
