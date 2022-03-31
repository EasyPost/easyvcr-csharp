using System.Net.Http;
using EasyPost.EasyVCR.Handlers;

namespace EasyPost.EasyVCR
{
    public static class HttpClients
    {
        /// <summary>
        ///     Get a new HttpClient configured to use cassettes.
        /// </summary>
        /// <param name="cassetteFolder">Folder where cassettes will be stored.</param>
        /// <param name="cassetteName">Name of the cassette to use.</param>
        /// <param name="mode">Mode to operate in (i.e. Record, Replay, Auto).</param>
        /// <param name="censors">Censors object to use when building requests and responses.</param>
        /// <param name="matchRules">MatchRules object to use when evaluating recordings.</param>
        /// <returns>An HttpClient object.</returns>
        public static HttpClient NewHttpClient(string cassetteFolder, string cassetteName, Mode mode, Censors? censors = null, MatchRules? matchRules = null)
        {
            return NewHttpClientWithHandler(null, cassetteFolder, cassetteName, mode, censors, matchRules);
        }

        /// <summary>
        ///     Get a new HttpClient configured to use cassettes.
        /// </summary>
        /// <param name="cassette">Cassette object to use.</param>
        /// <param name="mode">Mode to operate in (i.e. Record, Replay, Auto).</param>
        /// <param name="censors">Censors object to use when building requests and responses.</param>
        /// <param name="matchRules">MatchRules object to use when evaluating recordings.</param>
        /// <returns>An HttpClient object.</returns>
        public static HttpClient NewHttpClient(Cassette cassette, Mode mode, Censors? censors = null, MatchRules? matchRules = null)
        {
            return NewHttpClientWithHandler(null, cassette, mode, censors, matchRules);
        }

        /// <summary>
        ///     Get a new HttpClient configured to use cassettes.
        /// </summary>
        /// <param name="innerHandler">Custom inner handler to execute as part of HTTP requests.</param>
        /// <param name="cassetteFolder">Folder where cassettes will be stored.</param>
        /// <param name="cassetteName">Name of the cassette to use.</param>
        /// <param name="mode">Mode to operate in (i.e. Record, Replay, Auto).</param>
        /// <param name="censors">Censors object to use when building requests and responses.</param>
        /// <param name="matchRules">MatchRules object to use when evaluating recordings.</param>
        /// <returns>An HttpClient object.</returns>
        public static HttpClient NewHttpClientWithHandler(HttpMessageHandler? innerHandler, string cassetteFolder, string cassetteName, Mode mode, Censors? censors = null, MatchRules? matchRules = null)
        {
            return NewHttpClientWithHandler(innerHandler, new Cassette(cassetteFolder, cassetteName), mode, censors, matchRules);
        }

        /// <summary>
        ///     Get a new HttpClient configured to use cassettes.
        /// </summary>
        /// <param name="innerHandler">Custom inner handler to execute as part of HTTP requests.</param>
        /// <param name="cassette">Cassette object to use.</param>
        /// <param name="mode">Mode to operate in (i.e. Record, Replay, Auto).</param>
        /// <param name="censors">Censors object to use when building requests and responses.</param>
        /// <param name="matchRules">MatchRules object to use when evaluating recordings.</param>
        /// <returns>An HttpClient object.</returns>
        public static HttpClient NewHttpClientWithHandler(HttpMessageHandler? innerHandler, Cassette cassette, Mode mode, Censors? censors = null, MatchRules? matchRules = null)
        {
            innerHandler ??= new HttpClientHandler();

            switch (mode)
            {
                case Mode.Record:
                    innerHandler = new RecordingHandler(innerHandler, cassette, censors, matchRules);
                    break;
                case Mode.Replay:
                    innerHandler = new ReplayingHandler(innerHandler, cassette, censors, matchRules);
                    break;
                case Mode.Auto:
                    innerHandler = new AutoHandler(innerHandler, cassette, censors, matchRules);
                    break;
                case Mode.Bypass:
                default:
                    break;
            }

            return new HttpClient(innerHandler);
        }
    }
}
