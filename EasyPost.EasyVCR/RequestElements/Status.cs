using System.Net;
using Newtonsoft.Json;

namespace EasyPost.EasyVCR.RequestElements
{
    public class Status : HttpElement
    {
        [JsonProperty("Code")]
        public HttpStatusCode Code { get; set; }
        [JsonProperty("Message")]
        public string? Message { get; set; }
    }
}
