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

    public abstract class FakeDataService
    {
        private readonly HttpClient? _client;
        private readonly string? _format;
        private readonly VCR? _vcr;

        public HttpClient Client
        {
            get
            {
                if (_client != null) return _client;

                if (_vcr != null) return _vcr.Client;

                throw new InvalidOperationException("No VCR or HttpClient has been set.");
            }
        }

        protected FakeDataService(string format, VCR vcr)
        {
            _format = format;
            _vcr = vcr;
        }

        protected FakeDataService(string format, HttpClient client)
        {
            _format = format;
            _client = client;
        }

        public async Task<IPAddressData?> GetIPAddressData()
        {
            var response = await GetIPAddressDataRawResponse();
            return Convert(await response.Content.ReadAsStringAsync());
        }

        public async Task<HttpResponseMessage> GetIPAddressDataRawResponse()
        {
            return await Client.GetAsync(GetIPAddressDataUrl(_format));
        }

        protected abstract IPAddressData Convert(string responseBody);

        public static string GetIPAddressDataUrl(string? format)
        {
            return $"https://api.ipify.org/?format={format}";
        }
    }
}
