using System.Net.Http;
using EasyVCR.Handlers;

// ReSharper disable InconsistentNaming

namespace EasyVCR
{
    public class EasyVCRHttpClient : HttpClient
    {
        internal VCRHandler VcrHandler { get; }

        internal EasyVCRHttpClient(VCRHandler vcrHandler) : base(vcrHandler)
        {
            VcrHandler = vcrHandler;
        }

        /// <summary>
        ///     Create a clone of the current instance, with the same configuration.
        /// </summary>
        /// <returns>An EasyVcrHttpClient instance.</returns>
        internal EasyVCRHttpClient Clone()
        {
            return new EasyVCRHttpClient(VcrHandler);
        }

        public override bool Equals(object? obj)
        {
            if (obj is EasyVCRHttpClient other)
            {
                return VcrHandler.Equals(other.VcrHandler);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return VcrHandler.GetHashCode();
        }
    }
}
