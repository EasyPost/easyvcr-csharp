namespace EasyVCR
{
    public enum ExpirationActions
    {
        /// <summary>
        ///     Warn that the recorded interaction is expired, but proceed as normal.
        /// </summary>
        Warn,
        /// <summary>
        ///     Address the expiration.
        ///     In replay mode, this will throw an exception.
        ///     In auto mode, this will automatically re-record the interaction.
        /// </summary>
        Address
    }
}
