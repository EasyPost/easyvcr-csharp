using System.Collections.Generic;
using Newtonsoft.Json;

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
    }
}
