using System;

namespace EasyPost.EasyVCR
{
    public class VCRException : Exception
    {
        internal VCRException(string message) : base(message)
        {
        }
    }
}
