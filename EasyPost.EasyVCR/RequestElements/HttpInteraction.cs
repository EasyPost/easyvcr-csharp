using System;
using Newtonsoft.Json;

namespace EasyPost.EasyVCR.RequestElements
{
    internal class HttpInteraction
    {
        [JsonProperty("RecordedAt")]
        internal DateTimeOffset RecordedAt { get; set; }
        [JsonProperty("Request")]
        internal Request Request { get; set; }
        [JsonProperty("Response")]
        internal Response Response { get; set; }

        /// <summary>
        ///     Serialize this HttpInteraction object to a JSON string
        /// </summary>
        /// <returns>JSON string representation of this HttpInteraction object.</returns>
        internal string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
