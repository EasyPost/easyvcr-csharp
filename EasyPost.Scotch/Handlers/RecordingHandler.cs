using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EasyPost.Scotch.InternalUtilities;
using EasyPost.Scotch.RequestElements;

namespace EasyPost.Scotch.Handlers
{
    public class RecordingHandler : DelegatingHandler
    {
        private readonly Cassette _cassette;

        private readonly List<string>? _headersToHide;

        internal RecordingHandler(HttpMessageHandler innerHandler, Cassette cassette, List<string>? headersToHide = null)
        {
            InnerHandler = innerHandler;
            _cassette = cassette;
            _headersToHide = headersToHide;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var baseResult = base.SendAsync(request, cancellationToken);

            await Task.Run(async () =>
            {
                var response = await baseResult;
                var interactionRequest = await InteractionHelpers.ToRequestAsync(request, _headersToHide);
                var interactionResponse = await InteractionHelpers.ToResponseAsync(response, _headersToHide);
                var httpInteraction = new HttpInteraction
                {
                    Request = interactionRequest,
                    Response = interactionResponse,
                    RecordedAt = DateTimeOffset.Now
                };
                _cassette.UpdateInteraction(httpInteraction);
            });

            return baseResult.Result;
        }
    }
}
