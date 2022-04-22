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
        /// <param name="request">HttpRequestMessage object to convert.</param>
        /// <param name="censors">Censors to apply when creating the Request object.</param>
        /// <returns>Request object.</returns>
        public async Task<Request> ToRequestAsync(HttpRequestMessage request, Censors censors)
        {
            var requestBody = await ToStringAsync(request.Content);
            return new Request
            {
                Method = request.Method.ToString(),
                Uri = censors.CensorQueryParameters(request.RequestUri?.ToString()),
                RequestHeaders = censors.CensorHeaders(ToHeaders(request.Headers)),
                ContentHeaders = censors.CensorHeaders(ToContentHeaders(request.Content)),
                Body = censors.CensorBodyParameters(requestBody)
            };
        }

        /// <summary>
        ///     Convert an HttpResponseMessage to a Response object.
        /// </summary>
        /// <param name="response">HttpResponseMessage object to convert.</param>
        /// <param name="censors">Censors to apply when creating the Response object.</param>
        /// <returns>Response object.</returns>
        public async Task<Response> ToResponseAsync(HttpResponseMessage response, Censors censors)
        {
            var responseBody = await ToStringAsync(response.Content);
            return new Response
            {
                Status = new Status
                {
                    Code = response.StatusCode,
                    Message = response.ReasonPhrase
                },
                ResponseHeaders = censors.CensorHeaders(ToHeaders(response.Headers)),
                ContentHeaders = censors.CensorHeaders(ToContentHeaders(response.Content)),
                Body = censors.CensorBodyParameters(responseBody),
                HttpVersion = response.Version
            };
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
