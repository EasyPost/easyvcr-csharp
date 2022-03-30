using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyPost.EasyVCR.Tests
{
    [TestClass]
    public class ClientTest
    {
        [TestMethod]
        public void TestClient()
        {
            var cassette = TestUtils.GetCassette("test_client");
            var client = HttpClients.NewHttpClient(cassette, Mode.Bypass);

            Assert.IsNotNull(client);
        }

        [TestMethod]
        public void TestAlbumServiceClient()
        {
            var cassette = TestUtils.GetCassette("test_album_service_client");
            var client = HttpClients.NewHttpClient(cassette, Mode.Bypass);
            
            var albumService = new AlbumService(client);
            Assert.IsNotNull(albumService.Client);
        }

        [TestMethod]
        public async Task TestEraseAndRecord()
        {
            var cassette = TestUtils.GetCassette("test_erase_and_record");
            cassette.Erase(); // Erase cassette before recording
            Assert.IsTrue(cassette.Count == 0); // Make sure cassette is empty
            
            var client = HttpClients.NewHttpClient(cassette, Mode.Record);
            
            var albumService = new AlbumService(client);

            var albums = await albumService.GetAllAsync();
            Assert.IsNotNull(albums);
            Assert.AreEqual(albums.Count, 100);
            
            Assert.IsTrue(cassette.Count > 0); // Make sure cassette is not empty
        }
        
        [TestMethod]
        public async Task TestEraseAndPlayback()
        {
            var cassette = TestUtils.GetCassette("test_erase_and_playback");
            cassette.Erase(); // Erase cassette before recording
            Assert.IsTrue(cassette.Count == 0); // Make sure cassette is empty
            
            var client = HttpClients.NewHttpClient(cassette, Mode.Replay);
            
            var albumService = new AlbumService(client);

            await Assert.ThrowsExceptionAsync<VCRException>(async () => await albumService.GetAllAsync());
        }
    }
}
