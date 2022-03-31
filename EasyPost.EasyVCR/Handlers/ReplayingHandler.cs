using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EasyPost.EasyVCR.InternalUtilities;
using EasyPost.EasyVCR.RequestElements;

namespace EasyPost.EasyVCR.Handlers
{
    public class ReplayingHandler : DelegatingHandler
    {
        private readonly Cassette _cassette;
        private readonly Censors _censors;
        private readonly MatchRules _matchRules;

        internal ReplayingHandler(HttpMessageHandler innerHandler, Cassette cassette, Censors? censors = null, MatchRules? matchRules = null)
        {
            InnerHandler = innerHandler;
            _cassette = cassette;
            _censors = censors ?? Censors.Default;
            _matchRules = matchRules ?? MatchRules.Default;
        }

        /// <summary>
        ///     Override to alter the request-response behavior.
        ///     Replace the response to the request with the pre-recorded one.
        /// </summary>
        /// <param name="request">HttpRequestMessage object.</param>
        /// <param name="cancellationToken">CancellationToken object.</param>
        /// <returns>HttpResponseMessage object.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var interactions = _cassette.Read();
            var receivedRequest = await InteractionHelpers.ToRequestAsync(request, _censors);

            HttpInteraction matchedInteraction;

            try
            {
                matchedInteraction = interactions.First(i => _matchRules.RequestsMatch(receivedRequest, i.Request));
            }
            catch (InvalidOperationException)
            {
                throw new VCRException($"No interaction found for request {receivedRequest.Method} {receivedRequest.Uri}");
            }

            var matchedResponse = matchedInteraction.Response;
            var responseMessage = matchedResponse.ToHttpResponseMessage(request);
            return responseMessage;
        }
    }
}
