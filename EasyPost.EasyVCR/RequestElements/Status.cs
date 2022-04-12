using System.Net;
using Newtonsoft.Json;

namespace EasyPost.EasyVCR.RequestElements
{
    /// <summary>
    ///     Represents a status of an HTTP request tracked by EasyVCR.
    /// </summary>
    internal class Status : HttpElement
    {
        /// <summary>
        ///     The status code of the HTTP request.
        /// </summary>
        [JsonProperty("Code")]
        internal HttpStatusCode Code { get; set; }
        /// <summary>
        ///     The status description of the HTTP request.
        /// </summary>
        [JsonProperty("Message")]
        internal string? Message { get; set; }
    }
}
