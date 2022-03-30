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
    }
}
