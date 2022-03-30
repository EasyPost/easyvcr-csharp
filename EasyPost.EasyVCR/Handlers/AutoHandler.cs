using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EasyPost.EasyVCR.InternalUtilities;
using EasyPost.EasyVCR.RequestElements;

namespace EasyPost.EasyVCR.Handlers
{
    public class AutoHandler : RecordingHandler
    {
        internal AutoHandler(HttpMessageHandler innerHandler, Cassette cassette, List<string>? headersToHide = null, bool strictMatching = false) : base(innerHandler, cassette, headersToHide, strictMatching)
        {
        }

        /// <summary>
        /// Override to alter the request-response behavior.
        /// Use replay of request-response if it exists.
        /// Otherwise, record the request and response to the cassette.
        /// </summary>
        /// <param name="request">HttpRequestMessage object.</param>
        /// <param name="cancellationToken">CancellationToken object.</param>
        /// <returns>HttpResponseMessage object.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // use replay if available, otherwise record
            var existingInteraction = await FindMatchingInteraction(request);
            if (existingInteraction == null)
            {
                return await RecordNewInteraction(request, cancellationToken); // need to record a new interaction
            }

            // existing interaction found
            var matchedResponse = existingInteraction.Response;
            var responseMessage = matchedResponse.ToHttpResponseMessage(request);
            return responseMessage;
        }

        /// <summary>
        /// Search for an existing interaction that matches the request.
        /// </summary>
        /// <param name="request">HttpRequestMessage request.</param>
        /// <returns>Matching HttpInteraction or null if not found.</returns>
        private async Task<HttpInteraction?> FindMatchingInteraction(HttpRequestMessage request)
        {
            var interactions = Cassette.Read();
            var receivedRequest = await InteractionHelpers.ToRequestAsync(request);

            try
            {
                return interactions.First(i => InteractionHelpers.RequestsMatch(receivedRequest, i.Request));
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        /// <summary>
        /// Make a new HTTP request and record the request-response to the cassette.
        /// </summary>
        /// <param name="request">HttpRequestMessage object.</param>
        /// <param name="cancellationToken">CancellationToken object.</param>
        /// <returns>HttpResponseMessage object.</returns>
        private async Task<HttpResponseMessage> RecordNewInteraction(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var baseResult = base.SendAsync(request, cancellationToken);

            await Task.Run(async () =>
            {
                var response = await baseResult;
                var interactionRequest = await InteractionHelpers.ToRequestAsync(request, HeadersToHide);
                var interactionResponse = await InteractionHelpers.ToResponseAsync(response, HeadersToHide);
                var httpInteraction = new HttpInteraction
                {
                    Request = interactionRequest,
                    Response = interactionResponse,
                    RecordedAt = DateTimeOffset.Now
                };
                Cassette.UpdateInteraction(httpInteraction, StrictMatching);
            });

            return baseResult.Result;
        }
    }
}
