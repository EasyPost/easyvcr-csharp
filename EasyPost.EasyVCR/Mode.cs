namespace EasyPost.EasyVCR
{
    /// <summary>
    /// Modes of operation for the VCR system.
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// No recording or playback. Make HTTP requests as normal.
        /// </summary>
        Bypass,
        /// <summary>
        /// Replay existing recordings if they exist, otherwise record new ones.
        /// </summary>
        Auto,
        /// <summary>
        /// Always record new HTTP requests, including overwriting existing recordings.
        /// </summary>
        Record,
        /// <summary>
        /// Always replay existing recordings. Throw an exception if a recording does not exists.
        /// </summary>
        Replay
    }
}
