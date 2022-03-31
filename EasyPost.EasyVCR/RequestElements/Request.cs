using System.Collections.Generic;
using Newtonsoft.Json;

namespace EasyPost.EasyVCR.RequestElements
{
    public class Request : HttpElement
    {
        [JsonProperty("Body")]
        public string? Body { get; set; }
        [JsonProperty("ContentHeaders")]
        public IDictionary<string, string>? ContentHeaders { get; set; }
        [JsonProperty("Method")]
        public string Method { get; set; }
        [JsonProperty("RequestHeaders")]
        public IDictionary<string, string> RequestHeaders { get; set; }
        [JsonProperty("Uri")]
        public string? Uri { get; set; }
    }
}
