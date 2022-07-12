using System.Collections.Generic;
using EasyVCR.InternalUtilities;
using Newtonsoft.Json;

#pragma warning disable CS8618

namespace EasyVCR.RequestElements
{
    /// <summary>
    ///     Represents an HTTP request tracked by EasyVCR.
    /// </summary>
    public class Request : HttpElement
    {
        /// <summary>
        ///     The body of the request.
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
        ///     The content headers of the request.
        /// </summary>
        [JsonProperty("ContentHeaders")]
        internal IDictionary<string, string>? ContentHeaders { get; set; }
        /// <summary>
        ///     The method of the request.
        /// </summary>
        [JsonProperty("Method")]
        internal string Method { get; set; }
        /// <summary>
        ///     The request headers of the request.
        /// </summary>
        [JsonProperty("RequestHeaders")]
        internal IDictionary<string, string> RequestHeaders { get; set; }
        /// <summary>
        ///    The URL of the request.
        /// </summary>
        [JsonProperty("Uri")]
        internal string? Uri { get; set; }

        /// <summary>
        ///     The content type of the body of the response (string).
        /// </summary>
        [JsonProperty("BodyContentType")]
        private string? BodyContentTypeString { get; set; }
    }
}
