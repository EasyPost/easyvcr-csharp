using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyPost.Scotch.Tests
{
    [TestClass]
    public class VCRTest
    {
        private VCR _vcr;

        public VCRTest()
        {
            _vcr = new VCR();
        }

        [TestMethod]
        public async Task TestRecord()
        {
            var cassette = new Cassette("/Users/nharris/code/scotch/EasyPost.Scotch.Tests/", "TestRecord");
            _vcr.Insert(cassette);
            _vcr.Record();
            
            var response = await _vcr.Client.GetAsync("https://jsonplaceholder.typicode.com/albums");
        }
    }
}


