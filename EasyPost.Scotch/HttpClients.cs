using System.Collections.Generic;
using System.Net.Http;

namespace EasyPost.Scotch
{
    public static class HttpClients
    {
        private static List<string>? DefaultCredentialHeadersToHide => new()
        {
            {
                "Authorization"
            }
        };

        public static HttpClient NewHttpClient(string cassettePath, ScotchMode mode, bool hideCredentials = false, List<string>? headersToHide = null)
        {
            return NewHttpClientWithHandler(new HttpClientHandler(), cassettePath, mode, hideCredentials, headersToHide);
        }

        public static HttpClient NewHttpClientWithHandler(HttpMessageHandler innerHandler, string cassettePath, ScotchMode mode, bool hideCredentials = false, List<string>? headersToHide = null)
        {
            switch (mode)
            {
                case ScotchMode.Recording:
                    return new HttpClient(new RecordingHandler(innerHandler, cassettePath, hideCredentials ? (headersToHide ?? DefaultCredentialHeadersToHide) : null));
                case ScotchMode.Replaying:
                    return new HttpClient(new ReplayingHandler(innerHandler, cassettePath));
                case ScotchMode.None:
                default:
                    return new HttpClient(innerHandler);
            }
        }
    }
}
