using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EasyPost.Scotch.RequestElements;

namespace EasyPost.Scotch.InternalUtilities
{
    internal static class InteractionHelpers
    {
        internal static bool RequestsMatch(Request receivedRequest, Request recordedRequest)
        {
            return receivedRequest.Method.Equals(recordedRequest.Method, StringComparison.OrdinalIgnoreCase)
                   && receivedRequest.Uri.Equals(recordedRequest.Uri, StringComparison.OrdinalIgnoreCase);
        }

        internal static async Task<Request> ToRequestAsync(HttpRequestMessage request, List<string>? headersToHide = null)
        {
            var requestBody = await ToStringAsync(request.Content);
            return new Request
            {
                Method = request.Method.ToString(),
                Uri = request.RequestUri.ToString(),
                RequestHeaders = ToHeaders(request.Headers, headersToHide),
                ContentHeaders = ToContentHeaders(request.Content),
                Body = requestBody
            };
        }

        internal static async Task<Response> ToResponseAsync(HttpResponseMessage response, List<string>? headersToHide = null)
        {
            var responseBody = await ToStringAsync(response.Content);
            return new Response
            {
                Status = new Status
                {
                    Code = response.StatusCode,
                    Message = response.ReasonPhrase
                },
                ResponseHeaders = ToHeaders(response.Headers, headersToHide),
                ContentHeaders = ToContentHeaders(response.Content),
                Body = responseBody,
                HttpVersion = response.Version
            };
        }

        private static IDictionary<string, string> ToContentHeaders(HttpContent? content)
        {
            return content == null ? new Dictionary<string, string>() : ToHeaders(content.Headers);
        }

        private static IDictionary<string, string> ToHeaders(HttpHeaders headers, List<string>? headersToHide = null)
        {
            headersToHide ??= new List<string>();

            IDictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var h in headers) dict.Add(h.Key, headersToHide.Contains(h.Key) ? "********" : string.Join(",", h.Value));

            return dict;
        }

        private static async Task<string> ToStringAsync(HttpContent? content)
        {
            return content == null ? string.Empty : await content.ReadAsStringAsync();
        }
    }
}
