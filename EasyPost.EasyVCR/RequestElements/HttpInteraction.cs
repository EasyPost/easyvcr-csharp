using System;
using Newtonsoft.Json;

namespace EasyPost.EasyVCR.RequestElements
{
    public class HttpInteraction : HttpElement
    {
        [JsonProperty("RecordedAt")]
        public DateTimeOffset RecordedAt { get; set; }
        [JsonProperty("Request")]
        public Request Request { get; set; }
        [JsonProperty("Response")]
        public Response Response { get; set; }
    }
}
