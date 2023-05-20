using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace EasyVCR.Tests
{
    public class IPAddressData
    {
        #region JSON Properties

        [JsonProperty("ip")]
        internal string? IPAddress { get; set; }

        #endregion
    }

    public class FakeDataService
    {
        private readonly EasyVCRHttpClient? _client;
        private readonly VCR? _vcr;

        public EasyVCRHttpClient Client
        {
            get
            {
                if (_client != null) return _client;

                if (_vcr != null) return _vcr.Client;

                throw new InvalidOperationException("No VCR or EasyVcrHttpClient has been set.");
            }
        }

        public FakeDataService(VCR vcr)
        {
            _vcr = vcr;
        }

        public FakeDataService(EasyVCRHttpClient client)
        {
            _client = client;
        }

        public static string JsonDataUrl => "https://www.reddit.com/r/ProgrammerHumor.json";

        public static string XmlDataUrl => "https://www.reddit.com/r/ProgrammerHumor.rss";

        public static string HtmlDataUrl => "https://www.reddit.com/r/ProgrammerHumor";

        public static string RawDataUrl => "https://raw.githubusercontent.com/nwithan8/UGAArchive/main/README.md";

        public async Task<HttpResponseMessage> GetJsonDataRawResponse()
        {
            Client.DefaultRequestHeaders.Add("User-Agent", "EasyVCR"); // reddit requires a user agent
            return await Client.GetAsync(JsonDataUrl);
        }

        public async Task<string?> GetJsonData()
        {
            var response = await GetJsonDataRawResponse();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> GetXmlDataRawResponse()
        {
            Client.DefaultRequestHeaders.Add("User-Agent", "EasyVCR"); // reddit requires a user agent
            return await Client.GetAsync(XmlDataUrl);
        }

        public async Task<string?> GetXmlData()
        {
            var response = await GetXmlDataRawResponse();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> GetHtmlDataRawResponse()
        {
            Client.DefaultRequestHeaders.Add("User-Agent", "EasyVCR"); // reddit requires a user agent
            return await Client.GetAsync(HtmlDataUrl);
        }

        public async Task<string?> GetHtmlData()
        {
            var response = await GetHtmlDataRawResponse();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> GetRawDataRawResponse()
        {
            return await Client.GetAsync(RawDataUrl);
        }

        public async Task<string?> GetRawData()
        {
            var response = await GetRawDataRawResponse();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
