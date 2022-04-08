using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace EasyPost.EasyVCR.RequestElements
{
    /// <summary>
    ///     Represents an HTTP response tracked by EasyVCR.
    /// </summary>
    public class Response : HttpElement
    {
        /// <summary>
        ///     The body of the response.
        /// </summary>
        [JsonProperty("Body")]
        internal string? Body { get; set; }
        /// <summary>
        ///     The content headers of the response.
        /// </summary>
        [JsonProperty("ContentHeaders")]
        internal IDictionary<string, string>? ContentHeaders { get; set; }
        /// <summary>
        ///    The HTTP version of the response.
        /// </summary>
        [JsonProperty("HttpVersion")]
        internal Version HttpVersion { get; set; }
        /// <summary>
        ///     The response headers of the response.
        /// </summary>
        [JsonProperty("ResponseHeaders")]
        internal IDictionary<string, string>? ResponseHeaders { get; set; }
        /// <summary>
        ///     The status of the response.
        /// </summary>
        [JsonProperty("Status")]
        internal Status Status { get; set; }

        /// <summary>
        ///     Build an HttpResponseMessage out of an HttpRequestMessage object
        /// </summary>
        /// <param name="requestMessage">HttpRequestMessage object to use to build the HttpResponseMessage object.</param>
        /// <returns>An HttpResponseMessage object</returns>
        internal HttpResponseMessage ToHttpResponseMessage(HttpRequestMessage requestMessage)
        {
            var result = new HttpResponseMessage(Status.Code);
            result.ReasonPhrase = Status.Message;
            result.Version = HttpVersion;
            foreach (var h in ResponseHeaders ?? new Dictionary<string, string>()) result.Headers.TryAddWithoutValidation(h.Key, h.Value.ToString());

            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(Body ?? string.Empty));
            foreach (var h in ContentHeaders ?? new Dictionary<string, string>()) content.Headers.TryAddWithoutValidation(h.Key, h.Value.ToString());

            result.Content = content;
            result.RequestMessage = requestMessage;
            return result;
        }
    }
}
