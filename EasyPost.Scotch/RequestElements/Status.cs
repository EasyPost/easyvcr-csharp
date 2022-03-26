using System.Net;
using Newtonsoft.Json;

namespace EasyPost.Scotch.RequestElements
{
    internal class Status
    {
        [JsonProperty("Code")]
        internal HttpStatusCode Code { get; set; }
        [JsonProperty("Message")]
        internal string? Message { get; set; }
    }
}
