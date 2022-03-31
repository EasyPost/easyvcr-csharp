using System.Collections.Generic;
using Newtonsoft.Json;

namespace EasyPost.EasyVCR.RequestElements
{
    internal class Request
    {
        [JsonProperty("Body")]
        internal string? Body { get; set; }
        [JsonProperty("ContentHeaders")]
        internal IDictionary<string, string>? ContentHeaders { get; set; }
        [JsonProperty("Method")]
        internal string Method { get; set; }
        [JsonProperty("RequestHeaders")]
        internal IDictionary<string, string> RequestHeaders { get; set; }
        [JsonProperty("Uri")]
        internal string? Uri { get; set; }

        /// <summary>
        ///     Serialize this Request object to a JSON string
        /// </summary>
        /// <returns>JSON string representation of this Request object.</returns>
        internal string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
