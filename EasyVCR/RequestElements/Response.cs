using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using EasyVCR.InternalUtilities;
using Newtonsoft.Json;

#pragma warning disable CS8618

namespace EasyVCR.RequestElements
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
        ///     The content type of the body of the response.
        /// </summary>
        [JsonIgnore]
        internal ContentType? BodyContentType
        {
            get => ContentTypeExtensions.FromString(BodyContentTypeString);
            set => BodyContentTypeString = value?.ToString();
        }

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
        ///     The content type of the body of the response (string).
        /// </summary>
        [JsonProperty("BodyContentType")]
        private string? BodyContentTypeString { get; set; }

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
            // ReSharper disable once RedundantToStringCall
            foreach (var h in ResponseHeaders ?? new Dictionary<string, string>()) result.Headers.TryAddWithoutValidation(h.Key, h.Value.ToString());

            // add default replay headers
            foreach (var h in Defaults.ReplayHeaders) result.Headers.TryAddWithoutValidation(h.Key, h.Value.ToString());

            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(Body ?? string.Empty));
            // ReSharper disable once RedundantToStringCall
            foreach (var h in ContentHeaders ?? new Dictionary<string, string>()) content.Headers.TryAddWithoutValidation(h.Key, h.Value.ToString());

            result.Content = content;
            result.RequestMessage = requestMessage;
            return result;
        }
    }
}
