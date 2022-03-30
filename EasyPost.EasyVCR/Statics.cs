using System.Collections.Generic;

namespace EasyPost.EasyVCR
{
    public static class Statics
    {
        /// <summary>
        /// Default list of headers to censor in the cassettes.
        /// </summary>
        internal static List<string>? DefaultCredentialHeadersToHide => new()
        {
            {
                "Authorization"
            }
        };
    }
}
