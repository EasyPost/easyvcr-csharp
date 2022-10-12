using System.Net.Http;

// ReSharper disable UnusedMember.Global

namespace EasyVCR.Tests
{
    public class FakeJsonDataService : FakeDataService
    {
        public FakeJsonDataService(VCR vcr) : base("json", vcr)
        {
        }

        public FakeJsonDataService(EasyVCRHttpClient client) : base("json", client)
        {
        }

        protected override IPAddressData Convert(string responseBody)
        {
            return InternalUtilities.JSON.Serialization.ConvertJsonToObject<IPAddressData>(responseBody);
        }
    }

    public class FakeXmlDataService : FakeDataService
    {
        public FakeXmlDataService(VCR vcr) : base("xml", vcr)
        {
        }

        public FakeXmlDataService(EasyVCRHttpClient client) : base("xml", client)
        {
        }

        protected override IPAddressData Convert(string responseBody)
        {
            return InternalUtilities.XML.Serialization.ConvertXmlToObject<IPAddressData>(responseBody);
        }
    }
}
