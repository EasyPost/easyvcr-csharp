using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyPost.EasyVCR.Tests
{
    [TestClass]
    public class ClientTest
    {
        [TestMethod]
        public void TestFakeDataServiceClient()
        {
            var cassette = TestUtils.GetCassette("test_fake_data_service_client");
            var client = HttpClients.NewHttpClient(cassette, Mode.Bypass);

            var fakeDataService = new FakeDataService(client);
            Assert.IsNotNull(fakeDataService.Client);
        }

        [TestMethod]
        public void TestClient()
        {
            var cassette = TestUtils.GetCassette("test_client");
            var client = HttpClients.NewHttpClient(cassette, Mode.Bypass);

            Assert.IsNotNull(client);
        }

        [TestMethod]
        public async Task TestEraseAndPlayback()
        {
            var cassette = TestUtils.GetCassette("test_erase_and_playback");
            cassette.Erase(); // Erase cassette before recording
            Assert.IsTrue(cassette.NumInteractions == 0); // Make sure cassette is empty

            var client = HttpClients.NewHttpClient(cassette, Mode.Replay);

            var fakeDataService = new FakeDataService(client);

            await Assert.ThrowsExceptionAsync<VCRException>(async () => await fakeDataService.GetPosts());
        }

        [TestMethod]
        public async Task TestEraseAndRecord()
        {
            var cassette = TestUtils.GetCassette("test_erase_and_record");
            cassette.Erase(); // Erase cassette before recording
            Assert.IsTrue(cassette.NumInteractions == 0); // Make sure cassette is empty

            var client = HttpClients.NewHttpClient(cassette, Mode.Record);

            var fakeDataService = new FakeDataService(client);

            var posts = await fakeDataService.GetPosts();
            Assert.IsNotNull(posts);
            Assert.AreEqual(posts.Count, 100);

            Assert.IsTrue(cassette.NumInteractions > 0); // Make sure cassette is not empty
        }
    }
}
