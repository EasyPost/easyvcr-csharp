using System.Collections.Generic;

namespace EasyPost.EasyVCR
{
    /// <summary>
    ///     Static values used as defaults for EasyVCR.
    /// </summary>
    internal static class Statics
    {
        /// <summary>
        ///     Default list of headers to censor in the cassettes.
        /// </summary>
        internal static List<string> DefaultCredentialHeadersToHide => new()
        {
            {
                "Authorization"
            }
        };

        /// <summary>
        ///     Default list of parameters to censor in the cassettes.
        /// </summary>
        internal static List<string> DefaultCredentialParametersToHide => new()
        {
            {
                "api_key"
            },
            {
                "apiKey"
            },
            {
                "key"
            },
            {
                "api_token"
            },
            {
                "apiToken"
            },
            {
                "token"
            },
            {
                "access_token"
            },
            {
                "client_id"
            },
            {
                "client_secret"
            },
            {
                "password"
            },
            {
                "secret"
            },
            {
                "username"
            }
        };
    }
}
