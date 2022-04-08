using System;

namespace EasyPost.EasyVCR
{
    /// <summary>
    ///     Custom exception for EasyVCR
    /// </summary>
    public class VCRException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VCRException" /> class.
        /// </summary>
        /// <param name="message">Error message</param>
        internal VCRException(string message) : base(message)
        {
        }
    }
}
