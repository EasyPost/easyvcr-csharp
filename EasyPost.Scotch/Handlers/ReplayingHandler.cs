using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EasyPost.Scotch.InternalUtilities;
using EasyPost.Scotch.RequestElements;

namespace EasyPost.Scotch.Handlers
{
    public class ReplayingHandler : DelegatingHandler
    {
        private readonly Cassette _cassette;

        internal ReplayingHandler(HttpMessageHandler innerHandler, Cassette cassette)
        {
            InnerHandler = innerHandler;
            _cassette = cassette;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var interactions = _cassette.Read();
            var receivedRequest = await InteractionHelpers.ToRequestAsync(request);

            HttpInteraction matchedInteraction;

            try
            {
                matchedInteraction = interactions.First(i => InteractionHelpers.RequestsMatch(receivedRequest, i.Request));
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
