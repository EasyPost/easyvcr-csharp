using System.Collections.Generic;
using System.Net.Http;
using EasyPost.Scotch.Handlers;

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

        public static HttpClient NewHttpClient(string cassetteFolder, string cassetteName, ScotchMode mode, bool hideCredentials = false, List<string>? headersToHide = null)
        {
            return NewHttpClientWithHandler(null, cassetteFolder, cassetteName, mode, hideCredentials, headersToHide);
        }

        public static HttpClient NewHttpClient(Cassette cassette, ScotchMode mode, bool hideCredentials = false, List<string>? headersToHide = null)
        {
            return NewHttpClientWithHandler(null, cassette, mode, hideCredentials, headersToHide);
        }

        public static HttpClient NewHttpClientWithHandler(HttpMessageHandler? innerHandler, string cassetteFolder, string cassetteName, ScotchMode mode, bool hideCredentials = false, List<string>? headersToHide = null)
        {
            return NewHttpClientWithHandler(innerHandler, new Cassette(cassetteFolder, cassetteName), mode, hideCredentials, headersToHide);
        }

        public static HttpClient NewHttpClientWithHandler(HttpMessageHandler? innerHandler, Cassette cassette, ScotchMode mode, bool hideCredentials = false, List<string>? headersToHide = null)
        {
            innerHandler ??= new HttpClientHandler();
            switch (mode)
            {
                case ScotchMode.Recording:
                    return new HttpClient(new RecordingHandler(innerHandler, cassette, hideCredentials ? headersToHide ?? DefaultCredentialHeadersToHide : null));
                case ScotchMode.Replaying:
                    return new HttpClient(new ReplayingHandler(innerHandler, cassette));
                case ScotchMode.None:
                default:
                    return new HttpClient(innerHandler);
            }
        }
    }
}
