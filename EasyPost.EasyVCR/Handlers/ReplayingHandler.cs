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
        private readonly bool _strictMatching;

        internal ReplayingHandler(HttpMessageHandler innerHandler, Cassette cassette, bool strictMatching = false)
        {
            InnerHandler = innerHandler;
            _cassette = cassette;
            _strictMatching = strictMatching;
        }

        /// <summary>
        /// Override to alter the request-response behavior.
        /// Replace the response to the request with the pre-recorded one.
        /// </summary>
        /// <param name="request">HttpRequestMessage object.</param>
        /// <param name="cancellationToken">CancellationToken object.</param>
        /// <returns>HttpResponseMessage object.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var interactions = _cassette.Read();
            var receivedRequest = await InteractionHelpers.ToRequestAsync(request);

            HttpInteraction matchedInteraction;

            try
            {
                matchedInteraction = interactions.First(i => InteractionHelpers.RequestsMatch(receivedRequest, i.Request, _strictMatching));
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
