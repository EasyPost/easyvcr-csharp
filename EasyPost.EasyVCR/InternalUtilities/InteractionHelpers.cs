using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EasyPost.EasyVCR.RequestElements;

namespace EasyPost.EasyVCR.InternalUtilities
{
    internal static class InteractionHelpers
    {
        /// <summary>
        /// Check if two requests match by comparing the method and URI.
        /// </summary>
        /// <param name="receivedRequest">Request to find a match for.</param>
        /// <param name="recordedRequest">Request to compare against.</param>
        /// <param name="strict">Whether to check the body of the request.</param>
        /// <returns>True if requests match, false otherwise.</returns>
        internal static bool RequestsMatch(Request receivedRequest, Request recordedRequest, bool strict = false)
        {
            // convert uri to base64string to assist comparison by removing special characters
            var recordedUri = ToBase64String(recordedRequest.Uri);
            var receivedUri = ToBase64String(receivedRequest.Uri);
            var match = receivedRequest.Method.Equals(recordedRequest.Method, StringComparison.OrdinalIgnoreCase)
                   && receivedUri.Equals(recordedUri, StringComparison.OrdinalIgnoreCase);
            if (!strict || !match)
            {
                return match;
            }
            
            if (receivedRequest.Body == null && recordedRequest.Body == null)
            {
                // both have null bodies, so they match
                return true;
            }
            if (receivedRequest.Body == null || recordedRequest.Body == null)
            {
                // one has a null body, so they don't match
                return false;
            }
            // convert body to base64string to assist comparison by removing special characters
            var recordedBody = ToBase64String(recordedRequest.Body);
            var receivedBody = ToBase64String(receivedRequest.Body);
            return receivedBody.Equals(recordedBody, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Convert an HttpRequestMessage to a Request object.
        /// </summary>
        /// <param name="request">HttpRequestMessage object to convert.</param>
        /// <param name="headersToHide">List of headers to hide in the Request object.</param>
        /// <returns>Request object.</returns>
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

        /// <summary>
        /// Convert an HttpResponseMessage to a Response object.
        /// </summary>
        /// <param name="response">HttpResponseMessage object to convert.</param>
        /// <param name="headersToHide">List of headers to hide in the Response object.</param>
        /// <returns>Response object.</returns>
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

        /// <summary>
        /// Convert an HttpContent object's headers to a dictionary.
        /// </summary>
        /// <param name="content">HttpContent object to extract headers from.</param>
        /// <returns>Dictionary of strings pairs.</returns>
        private static IDictionary<string, string> ToContentHeaders(HttpContent? content)
        {
            return content == null ? new Dictionary<string, string>() : ToHeaders(content.Headers);
        }

        /// <summary>
        /// Convert an HttpHeaders object to a dictionary.
        /// </summary>
        /// <param name="headers">HttpHeaders object to extract headers from.</param>
        /// <param name="headersToHide">List of headers to hide in the resulting dictionary.</param>
        /// <returns>Dictionary of string pairs.</returns>
        private static IDictionary<string, string> ToHeaders(HttpHeaders headers, List<string>? headersToHide = null)
        {
            headersToHide ??= new List<string>();

            IDictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var h in headers) dict.Add(h.Key, headersToHide.Contains(h.Key) ? "********" : string.Join(",", h.Value));

            return dict;
        }

        /// <summary>
        /// Convert an HttpContent object to a string.
        /// </summary>
        /// <param name="content">HttpContent object to convert to a string.</param>
        /// <returns>String representation of the HttpContent object.</returns>
        private static async Task<string> ToStringAsync(HttpContent? content)
        {
            return content == null ? string.Empty : await content.ReadAsStringAsync();
        }
        
        /// <summary>
        /// Convert a string to a base64 encoded string.
        /// </summary>
        /// <param name="input">String to be encoded.</param>
        /// <returns>A base64 encoded string.</returns>
        private static string ToBase64String(string input)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(input));
        }
    }
}
