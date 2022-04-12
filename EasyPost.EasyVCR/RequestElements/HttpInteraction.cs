using System;
using Newtonsoft.Json;

namespace EasyPost.EasyVCR.RequestElements
{
    /// <summary>
    ///     Represents an HTTP request-response pair tracked by EasyVCR.
    /// </summary>
    internal class HttpInteraction : HttpElement
    {
        /// <summary>
        ///     Timestamp of when the interaction was recorded.
        /// </summary>
        [JsonProperty("RecordedAt")]
        internal DateTimeOffset RecordedAt { get; set; }
        /// <summary>
        ///     The HTTP request.
        /// </summary>
        [JsonProperty("Request")]
        internal Request Request { get; set; }
        /// <summary>
        ///     The HTTP response.
        /// </summary>
        [JsonProperty("Response")]
        internal Response Response { get; set; }
        /// <summary>
        ///     The duration of the request.
        /// </summary>
        [JsonProperty("Duration")]
        // request duration in milliseconds
        internal int Duration { get; set; }
    }
}
