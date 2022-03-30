using System.Collections.Generic;
using System.Net.Http;
using EasyPost.EasyVCR.Handlers;

namespace EasyPost.EasyVCR
{
    public static class HttpClients
    {
        /// <summary>
        /// Get a new HttpClient configured to use cassettes.
        /// </summary>
        /// <param name="cassetteFolder">Folder where cassettes will be stored.</param>
        /// <param name="cassetteName">Name of the cassette to use.</param>
        /// <param name="mode">Mode to operate in (i.e. Record, Replay, Auto).</param>
        /// <param name="hideCredentials">Whether to censor credentials in the cassette files.</param>
        /// <param name="headersToHide">Override the default list of credentials to censor in the cassette files.</param>
        /// <param name="strictMatching">Whether to match requests using strict matching.</param>
        /// <returns>An HttpClient object.</returns>
        public static HttpClient NewHttpClient(string cassetteFolder, string cassetteName, Mode mode, bool hideCredentials = false, List<string>? headersToHide = null, bool strictMatching = false)
        {
            return NewHttpClientWithHandler(null, cassetteFolder, cassetteName, mode, hideCredentials, headersToHide);
        }

        /// <summary>
        /// Get a new HttpClient configured to use cassettes.
        /// </summary>
        /// <param name="cassette">Cassette object to use.</param>
        /// <param name="mode">Mode to operate in (i.e. Record, Replay, Auto).</param>
        /// <param name="hideCredentials">Whether to censor credentials in the cassette files.</param>
        /// <param name="headersToHide">Override the default list of credentials to censor in the cassette files.</param>
        /// <param name="strictMatching">Whether to match requests using strict matching.</param>
        /// <returns>An HttpClient object.</returns>
        public static HttpClient NewHttpClient(Cassette cassette, Mode mode, bool hideCredentials = false, List<string>? headersToHide = null, bool strictMatching = false)
        {
            return NewHttpClientWithHandler(null, cassette, mode, hideCredentials, headersToHide);
        }

        /// <summary>
        /// Get a new HttpClient configured to use cassettes.
        /// </summary>
        /// <param name="innerHandler">Custom inner handler to execute as part of HTTP requests.</param>
        /// <param name="cassetteFolder">Folder where cassettes will be stored.</param>
        /// <param name="cassetteName">Name of the cassette to use.</param>
        /// <param name="mode">Mode to operate in (i.e. Record, Replay, Auto).</param>
        /// <param name="hideCredentials">Whether to censor credentials in the cassette files.</param>
        /// <param name="headersToHide">Override the default list of credentials to censor in the cassette files.</param>
        /// <param name="strictMatching">Whether to match requests using strict matching.</param>
        /// <returns>An HttpClient object.</returns>
        public static HttpClient NewHttpClientWithHandler(HttpMessageHandler? innerHandler, string cassetteFolder, string cassetteName, Mode mode, bool hideCredentials = false, List<string>? headersToHide = null, bool strictMatching = false)
        {
            return NewHttpClientWithHandler(innerHandler, new Cassette(cassetteFolder, cassetteName), mode, hideCredentials, headersToHide);
        }

        /// <summary>
        /// Get a new HttpClient configured to use cassettes.
        /// </summary>
        /// <param name="innerHandler">Custom inner handler to execute as part of HTTP requests.</param>
        /// <param name="cassette">Cassette object to use.</param>
        /// <param name="mode">Mode to operate in (i.e. Record, Replay, Auto).</param>
        /// <param name="hideCredentials">Whether to censor credentials in the cassette files.</param>
        /// <param name="headersToHide">Override the default list of credentials to censor in the cassette files.</param>
        /// <param name="strictMatching">Whether to match requests using strict matching.</param>
        /// <returns>An HttpClient object.</returns>
        public static HttpClient NewHttpClientWithHandler(HttpMessageHandler? innerHandler, Cassette cassette, Mode mode, bool hideCredentials = false, List<string>? headersToHide = null, bool strictMatching = false)
        {
            innerHandler ??= new HttpClientHandler();

            switch (mode)
            {
                case Mode.Record:
                    innerHandler = new RecordingHandler(innerHandler, cassette, hideCredentials ? headersToHide ?? Statics.DefaultCredentialHeadersToHide : null, strictMatching);
                    break;
                case Mode.Replay:
                    innerHandler = new ReplayingHandler(innerHandler, cassette, strictMatching);
                    break;
                case Mode.Auto:
                    innerHandler = new AutoHandler(innerHandler, cassette, hideCredentials ? headersToHide ?? Statics.DefaultCredentialHeadersToHide : null, strictMatching);
                    break;
                case Mode.Bypass:
                default:
                    break;
            }

            return new HttpClient(innerHandler);
        }
    }
}
