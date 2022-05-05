using System.Net.Http;

namespace EasyVCR.Tests
{
    public class FakeJsonDataService : FakeDataService
    {
        public FakeJsonDataService(VCR vcr) : base("json", vcr)
        {
        }

        public FakeJsonDataService(HttpClient client) : base("json", client)
        {
        }

        protected override ExchangeRates Convert(string responseBody)
        {
            return InternalUtilities.JSON.Serialization.ConvertJsonToObject<ExchangeRates>(responseBody);
        }
    }

    public class FakeXmlDataService : FakeDataService
    {
        public FakeXmlDataService(VCR vcr) : base("xml", vcr)
        {
        }

        public FakeXmlDataService(HttpClient client) : base("xml", client)
        {
        }

        protected override ExchangeRates Convert(string responseBody)
        {
            return InternalUtilities.XML.Serialization.ConvertXmlToObject<ExchangeRates>(responseBody);
        }
    }
}
