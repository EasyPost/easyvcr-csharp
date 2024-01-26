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
        private readonly EasyVCRHttpClient? _client;
        private readonly string? _format;
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

        protected FakeDataService(string format, VCR vcr)
        {
            _format = format;
            _vcr = vcr;
        }

        protected FakeDataService(string format, EasyVCRHttpClient client)
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
            return await Client.GetAsync(GetPreparedIPAddressDataUrl(_format));
        }

        public static string GetPreparedIPAddressDataUrl(string? format)
        {
            return $"{GetIPAddressDataUrl()}?format={format}";
        }

        protected abstract IPAddressData Convert(string responseBody);

        public static string GetIPAddressDataUrl()
        {
            return "https://api.ipify.org/";
        }

        public static string GetPostManPostEchoServiceUrl()
        {
            return "https://postman-echo.com/post";
        }
    }
}
