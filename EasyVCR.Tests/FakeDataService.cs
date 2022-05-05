using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EasyVCR.Tests
{
    public class ExchangeRates
    {
        [JsonProperty("code")]
        internal string Code { get; set; }
        [JsonProperty("currency")]
        internal string Currency { get; set; }
        [JsonProperty("rates")]
        internal List<Rate> Rates { get; set; }
        [JsonProperty("table")]
        internal string Table { get; set; }
    }

    public class Rate
    {
        [JsonProperty("effectiveDate")]
        internal string EffectiveDate { get; set; }
        [JsonProperty("mid")]
        internal float Mid { get; set; }
        [JsonProperty("no")]
        internal string No { get; set; }
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

        public FakeDataService(string format, VCR vcr)
        {
            _format = format;
            _vcr = vcr;
        }

        public FakeDataService(string format, HttpClient client)
        {
            _format = format;
            _client = client;
        }

        public async Task<ExchangeRates?> GetExchangeRates()
        {
            var response = await GetExchangeRatesRawResponse();
            return Convert(await response.Content.ReadAsStringAsync());
        }

        public async Task<HttpResponseMessage> GetExchangeRatesRawResponse()
        {
            return await Client.GetAsync("http://api.nbp.pl/api/exchangerates/rates/a/gbp/last/10/?format=" + _format);
        }

        protected abstract ExchangeRates Convert(string responseBody);
    }
}
