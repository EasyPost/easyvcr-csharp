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
        /// <param name="censors">Censors to use when building requests and responses.</param>
        /// <param name="matchRules">Advanced. Override rules used when evaluating recordings.</param>
        /// <param name="customInteractionConverter">
        ///     Advanced. Override how HttpRequestMessage and HttpResponseMessage objects are converted to Request and Response
        ///     objects.
        ///     Not recommended to use unless System.Net.Http has introduced breaking changes.
        /// </param>
        /// <returns>An HttpClient object.</returns>
        public static HttpClient NewHttpClient(string cassetteFolder, string cassetteName, Mode mode, Censors? censors = null, MatchRules? matchRules = null, IInteractionConverter? customInteractionConverter = null)
        {
            return NewHttpClientWithHandler(null, cassetteFolder, cassetteName, mode, censors, matchRules, customInteractionConverter);
        }

        /// <summary>
        ///     Get a new HttpClient configured to use cassettes.
        /// </summary>
        /// <param name="cassette">Cassette object to use.</param>
        /// <param name="mode">Mode to operate in (i.e. Record, Replay, Auto).</param>
        /// <param name="censors">Censors to use when building requests and responses.</param>
        /// <param name="matchRules">Advanced. Override rules used when evaluating recordings.</param>
        /// <param name="customInteractionConverter">
        ///     Advanced. Override how HttpRequestMessage and HttpResponseMessage objects are converted to Request and Response
        ///     objects.
        ///     Not recommended to use unless System.Net.Http has introduced breaking changes.
        /// </param>
        /// <returns>An HttpClient object.</returns>
        public static HttpClient NewHttpClient(Cassette cassette, Mode mode, Censors? censors = null, MatchRules? matchRules = null, IInteractionConverter? customInteractionConverter = null)
        {
            return NewHttpClientWithHandler(null, cassette, mode, censors, matchRules, customInteractionConverter);
        }

        /// <summary>
        ///     Get a new HttpClient configured to use cassettes.
        /// </summary>
        /// <param name="innerHandler">Custom inner handler to execute as part of HTTP requests.</param>
        /// <param name="cassetteFolder">Folder where cassettes will be stored.</param>
        /// <param name="cassetteName">Name of the cassette to use.</param>
        /// <param name="mode">Mode to operate in (i.e. Record, Replay, Auto).</param>
        /// <param name="censors">Censors to use when building requests and responses.</param>
        /// <param name="matchRules">Advanced. Override rules used when evaluating recordings.</param>
        /// <param name="customInteractionConverter">
        ///     Advanced. Override how HttpRequestMessage and HttpResponseMessage objects are converted to Request and Response
        ///     objects.
        ///     Not recommended to use unless System.Net.Http has introduced breaking changes.
        /// </param>
        /// <returns>An HttpClient object.</returns>
        public static HttpClient NewHttpClientWithHandler(HttpMessageHandler? innerHandler, string cassetteFolder, string cassetteName, Mode mode, Censors? censors = null, MatchRules? matchRules = null, IInteractionConverter? customInteractionConverter = null)
        {
            return NewHttpClientWithHandler(innerHandler, new Cassette(cassetteFolder, cassetteName), mode, censors, matchRules, customInteractionConverter);
        }

        /// <summary>
        ///     Get a new HttpClient configured to use cassettes.
        /// </summary>
        /// <param name="innerHandler">Custom inner handler to execute as part of HTTP requests.</param>
        /// <param name="cassette">Cassette object to use.</param>
        /// <param name="mode">Mode to operate in (i.e. Record, Replay, Auto).</param>
        /// <param name="censors">Censors to use when building requests and responses.</param>
        /// <param name="matchRules">Advanced. Override rules used when evaluating recordings.</param>
        /// <param name="customInteractionConverter">
        ///     Advanced. Override how HttpRequestMessage and HttpResponseMessage objects are converted to Request and Response
        ///     objects.
        ///     Not recommended to use unless System.Net.Http has introduced breaking changes.
        /// </param>
        /// <returns>An HttpClient object.</returns>
        public static HttpClient NewHttpClientWithHandler(HttpMessageHandler? innerHandler, Cassette cassette, Mode mode, Censors? censors = null, MatchRules? matchRules = null, IInteractionConverter? customInteractionConverter = null)
        {
            innerHandler ??= new HttpClientHandler();
            return new HttpClient(new VCRHandler(innerHandler, cassette, mode, censors, matchRules, customInteractionConverter));
        }
    }
}
