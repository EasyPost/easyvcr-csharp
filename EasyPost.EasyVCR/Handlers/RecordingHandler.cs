using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EasyPost.EasyVCR.InternalUtilities;
using EasyPost.EasyVCR.RequestElements;

namespace EasyPost.EasyVCR.Handlers
{
    public class RecordingHandler : DelegatingHandler
    {
        protected readonly Cassette Cassette;

        protected readonly List<string>? HeadersToHide;
        
        protected readonly bool StrictMatching;

        internal RecordingHandler(HttpMessageHandler innerHandler, Cassette cassette, List<string>? headersToHide = null, bool strictMatching = false)
        {
            InnerHandler = innerHandler;
            Cassette = cassette;
            HeadersToHide = headersToHide;
            StrictMatching = strictMatching;
        }

        /// <summary>
        /// Override to alter the request-response behavior.
        /// Record the request and response to the cassette.
        /// </summary>
        /// <param name="request">HttpRequestMessage object.</param>
        /// <param name="cancellationToken">CancellationToken object.</param>
        /// <returns>HttpResponseMessage object.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
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
