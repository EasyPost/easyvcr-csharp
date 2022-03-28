using System;

namespace EasyPost.Scotch
{
    public class VCRException : Exception
    {
        internal VCRException(string message) : base(message)
        {
        }
    }
}
