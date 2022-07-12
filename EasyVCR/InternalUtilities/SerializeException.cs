using System;

namespace EasyVCR.InternalUtilities
{
    /// <summary>
    ///     Custom serialization exception for EasyVCR
    /// </summary>
    internal abstract class SerializeException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VCRException" /> class.
        /// </summary>
        /// <param name="message">Error message</param>
        internal SerializeException(string message) : base(message)
        {
        }
    }
}
