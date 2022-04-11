using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyPost.EasyVCR.Tests
{
    [TestClass]
    public class VCRTest
    {
        private VCR _vcr;

        public VCRTest()
        {
            var settings = new AdvancedSettings
            {
                Censors = null
            };
            _vcr = new VCR(settings);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _vcr.Eject();
        }

        [TestMethod]
        public void TestClient()
        {
            var cassette = TestUtils.GetCassette("test_vcr_client");
            _vcr.Insert(cassette);

            Assert.IsNotNull(_vcr.Client);
        }

        [TestMethod]
        public void TestClientHandOff()
        {
            var cassette = TestUtils.GetCassette("test_vcr_mode_hand_off");
            _vcr.Insert(cassette);

            // test that we can still control the VCR even after it's been handed off to the service using it
            var fakeDataService = new FakeDataService(_vcr);
            Assert.IsNotNull(fakeDataService.Client); // Client should come from VCR, which has a client because it has a cassette.
            _vcr.Eject();
            Assert.ThrowsException<InvalidOperationException>(() => fakeDataService.Client); // Client should be null because the VCR's cassette has been ejected.
        }

        [TestMethod]
        public void TestClientNoCassette()
        {
            Assert.ThrowsException<InvalidOperationException>(() => _vcr.Client);
        }

        [TestMethod]
        public void TestEjectCassette()
        {
            var cassette = TestUtils.GetCassette("test_vcr_eject_cassette");
            _vcr.Insert(cassette);
            Assert.IsNotNull(_vcr.CassetteName);
            _vcr.Eject();
            Assert.IsNull(_vcr.CassetteName);
        }

        [TestMethod]
        public void TestInsertCassette()
        {
            var cassette = TestUtils.GetCassette("test_vcr_insert_cassette");
            _vcr.Insert(cassette);
            Assert.AreEqual(cassette.Name, _vcr.CassetteName);
        }

        [TestMethod]
        public void TestMode()
        {
            var cassette = TestUtils.GetCassette("test_vcr_mode");
            _vcr.Insert(cassette);
            _vcr.Record();
            Assert.AreEqual(_vcr.Mode, Mode.Record);
            _vcr.Replay();
            Assert.AreEqual(_vcr.Mode, Mode.Replay);
            _vcr.Pause();
            Assert.AreEqual(_vcr.Mode, Mode.Bypass);
        }

        [TestMethod]
        public async Task TestRecord()
        {
            var cassette = TestUtils.GetCassette("test_vcr_record");
            _vcr.Insert(cassette);
            var fakeDataService = new FakeDataService(_vcr);

            _vcr.Record();

            var posts = await fakeDataService.GetPosts();
            Assert.IsNotNull(posts);
            Assert.AreEqual(posts.Count, 100);
        }

        [TestMethod]
        public async Task TestReplay()
        {
            // make a new cassette with the same filename to reuse existing records
            var cassette = TestUtils.GetCassette("test_vcr_record");
            _vcr.Insert(cassette);
            var fakeDataService = new FakeDataService(_vcr);

            _vcr.Replay();

            var posts = await fakeDataService.GetPosts();
            Assert.IsNotNull(posts);
            Assert.AreEqual(posts.Count, 100);
        }

        //TODO: test different match rules

        //TODO: test bypass mode
    }
}
