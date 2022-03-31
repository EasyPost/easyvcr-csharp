using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace EasyPost.EasyVCR.RequestElements
{
    public class Response : HttpElement
    {
        [JsonProperty("Body")]
        public string? Body { get; set; }
        [JsonProperty("ContentHeaders")]
        public IDictionary<string, string>? ContentHeaders { get; set; }
        [JsonProperty("HttpVersion")]
        public Version HttpVersion { get; set; }
        [JsonProperty("ResponseHeaders")]
        public IDictionary<string, string>? ResponseHeaders { get; set; }
        [JsonProperty("Status")]
        public Status Status { get; set; }

        /// <summary>
        ///     Build an HttpResponseMessage out of an HttpRequestMessage object
        /// </summary>
        /// <param name="requestMessage">HttpRequestMessage object to use to build the HttpResponseMessage object.</param>
        /// <returns>An HttpResponseMessage object</returns>
        public HttpResponseMessage ToHttpResponseMessage(HttpRequestMessage requestMessage)
        {
            var result = new HttpResponseMessage(Status.Code);
            result.ReasonPhrase = Status.Message;
            result.Version = HttpVersion;
            foreach (var h in ResponseHeaders ?? new Dictionary<string, string>()) result.Headers.TryAddWithoutValidation(h.Key, h.Value.ToString());

            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(Body ?? string.Empty));
            foreach (var h in ContentHeaders ?? new Dictionary<string, string>()) content.Headers.TryAddWithoutValidation(h.Key, h.Value.ToString());

            result.Content = content;
            result.RequestMessage = requestMessage;
            return result;
        }
    }
}
