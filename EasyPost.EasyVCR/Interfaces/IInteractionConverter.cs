using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EasyPost.EasyVCR.RequestElements;

namespace EasyPost.EasyVCR.Interfaces
{
    /// <summary>
    ///     Interface for custom interaction converters to convert HttpClient requests/responses to/from EasyVCR requests/responses.
    /// </summary>
    public interface IInteractionConverter
    {
        /// <summary>
        ///     Convert an HttpContent object's headers to a dictionary.
        /// </summary>
        /// <param name="content">HttpContent object to extract headers from.</param>
        /// <returns>Dictionary of strings pairs.</returns>
        public IDictionary<string, string> ToContentHeaders(HttpContent? content);

        /// <summary>
        ///     Convert an HttpHeaders object to a dictionary.
        /// </summary>
        /// <param name="headers">HttpHeaders object to extract headers from.</param>
        /// <returns>Dictionary of string pairs.</returns>
        public IDictionary<string, string> ToHeaders(HttpHeaders headers);

        /// <summary>
        ///     Convert an HttpRequestMessage to a Request object.
        /// </summary>
        /// <param name="request">HttpRequestMessage object to convert.</param>
        /// <param name="censors">Censors to apply when creating the Request object.</param>
        /// <returns>Request object.</returns>
        public Task<Request> ToRequestAsync(HttpRequestMessage request, Censors censors);

        /// <summary>
        ///     Convert an HttpResponseMessage to a Response object.
        /// </summary>
        /// <param name="response">HttpResponseMessage object to convert.</param>
        /// <param name="censors">Censors to apply when creating the Response object.</param>
        /// <returns>Response object.</returns>
        public Task<Response> ToResponseAsync(HttpResponseMessage response, Censors censors);

        /// <summary>
        ///     Convert an HttpContent object to a string.
        /// </summary>
        /// <param name="content">HttpContent object to convert to a string.</param>
        /// <returns>String representation of the HttpContent object.</returns>
        public Task<string> ToStringAsync(HttpContent? content);
    }
}
