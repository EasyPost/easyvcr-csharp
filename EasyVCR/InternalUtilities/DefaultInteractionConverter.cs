using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EasyVCR.Interfaces;
using EasyVCR.RequestElements;

namespace EasyVCR.InternalUtilities
{
    /// <summary>
    ///     The default interaction converter to convert HttpClient requests/responses to/from EasyVCR requests/responses.
    /// </summary>
    internal class DefaultInteractionConverter : IInteractionConverter
    {
        /// <summary>
        ///     Convert an HttpContent object's headers to a dictionary.
        /// </summary>
        /// <param name="content">HttpContent object to extract headers from.</param>
        /// <returns>Dictionary of strings pairs.</returns>
        public IDictionary<string, string> ToContentHeaders(HttpContent? content)
        {
            return content == null ? new Dictionary<string, string>() : ToHeaders(content.Headers);
        }

        /// <summary>
        ///     Convert an HttpHeaders object to a dictionary.
        /// </summary>
        /// <param name="headers">HttpHeaders object to extract headers from.</param>
        /// <returns>Dictionary of string pairs.</returns>
        public IDictionary<string, string> ToHeaders(HttpHeaders headers)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var h in headers) dict.Add(h.Key, string.Join(",", h.Value));
            return dict;
        }

        /// <summary>
        ///     Convert an HttpRequestMessage to a Request object.
        /// </summary>
        /// <param name="httpRequestMessage">HttpRequestMessage object to convert.</param>
        /// <param name="censors">Censors to apply when creating the Request object.</param>
        /// <returns>Request object.</returns>
        public async Task<Request> ToRequestAsync(HttpRequestMessage httpRequestMessage, Censors censors)
        {
            var requestBody = await ToStringAsync(httpRequestMessage.Content);
            var request = new Request
            {
                Method = httpRequestMessage.Method.ToString(),
                Uri = censors.CensorQueryParameters(httpRequestMessage.RequestUri?.ToString()),
                RequestHeaders = censors.CensorHeaders(ToHeaders(httpRequestMessage.Headers)),
                ContentHeaders = censors.CensorHeaders(ToContentHeaders(httpRequestMessage.Content)),
                BodyContentType = ContentTypeExtensions.DetermineContentType(requestBody)
            };
            request.Body = censors.CensorBodyParameters(requestBody, request.BodyContentType);
            return request;
        }

        /// <summary>
        ///     Convert an HttpResponseMessage to a Response object.
        /// </summary>
        /// <param name="httpResponseMessage">HttpResponseMessage object to convert.</param>
        /// <param name="censors">Censors to apply when creating the Response object.</param>
        /// <returns>Response object.</returns>
        public async Task<Response> ToResponseAsync(HttpResponseMessage httpResponseMessage, Censors censors)
        {
            var responseBody = await ToStringAsync(httpResponseMessage.Content);
            var response = new Response
            {
                Status = new Status
                {
                    Code = httpResponseMessage.StatusCode,
                    Message = httpResponseMessage.ReasonPhrase
                },
                ResponseHeaders = censors.CensorHeaders(ToHeaders(httpResponseMessage.Headers)),
                ContentHeaders = censors.CensorHeaders(ToContentHeaders(httpResponseMessage.Content)),
                BodyContentType = ContentTypeExtensions.DetermineContentType(responseBody),
                HttpVersion = httpResponseMessage.Version
            };
            response.Body = censors.CensorBodyParameters(responseBody, response.BodyContentType);
            return response;
        }

        /// <summary>
        ///     Convert an HttpContent object to a string.
        /// </summary>
        /// <param name="content">HttpContent object to convert to a string.</param>
        /// <returns>String representation of the HttpContent object.</returns>
        public async Task<string> ToStringAsync(HttpContent? content)
        {
            return content == null ? string.Empty : await content.ReadAsStringAsync();
        }
    }
}
