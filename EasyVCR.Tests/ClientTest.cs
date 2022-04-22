using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyVCR.Tests
{
    [TestClass]
    public class ClientTest
    {
        private static async Task<List<Post>?> GetPostsRequest(Cassette cassette, Mode mode)
        {
            var client = HttpClients.NewHttpClient(cassette, mode);

            var fakeDataService = new FakeDataService(client);

            return await fakeDataService.GetPosts();
        }

        [TestMethod]
        public void TestClient()
        {
            var client = TestUtils.GetSimpleClient("test_client", Mode.Bypass);

            Assert.IsNotNull(client);
        }

        [TestMethod]
        public void TestFakeDataServiceClient()
        {
            var client = TestUtils.GetSimpleClient("test_fake_data_service_client", Mode.Bypass);

            var fakeDataService = new FakeDataService(client);
            Assert.IsNotNull(fakeDataService.Client);
        }

        public async Task TestErase()
        {
            var cassette = TestUtils.GetCassette("test_erase");

            // record something to the cassette
            var _ = await GetPostsRequest(cassette, Mode.Record);
            Assert.IsTrue(cassette.NumInteractions > 0);

            // erase the cassette
            cassette.Erase();
            Assert.IsTrue(cassette.NumInteractions == 0);
        }

        [TestMethod]
        public async Task TestEraseAndRecord()
        {
            var cassette = TestUtils.GetCassette("test_erase_and_record");
            cassette.Erase(); // Erase cassette before recording

            var posts = await GetPostsRequest(cassette, Mode.Record);

            Assert.IsNotNull(posts);
            Assert.AreEqual(posts.Count, 100);
            Assert.IsTrue(cassette.NumInteractions > 0); // Make sure cassette is not empty
        }

        [TestMethod]
        public async Task TestEraseAndPlayback()
        {
            var cassette = TestUtils.GetCassette("test_erase_and_record");
            cassette.Erase(); // Erase cassette before recording

            // cassette is empty, so replaying should throw an exception
            await Assert.ThrowsExceptionAsync<VCRException>(async () => await GetPostsRequest(cassette, Mode.Replay));
        }

        [TestMethod]
        public async Task TestAutoMode()
        {
            var cassette = TestUtils.GetCassette("test_auto_mode");
            cassette.Erase(); // Erase cassette before recording

            // in replay mode, if cassette is empty, should throw an exception
            await Assert.ThrowsExceptionAsync<VCRException>(async () => await GetPostsRequest(cassette, Mode.Replay));
            Assert.IsTrue(cassette.NumInteractions == 0); // Make sure cassette is still empty

            // in auto mode, if cassette is empty, should make and record a real request
            var posts = await GetPostsRequest(cassette, Mode.Auto);
            Assert.IsNotNull(posts);
            Assert.IsTrue(cassette.NumInteractions > 0); // Make sure cassette is no longer empty
        }


        [TestMethod]
        public async Task TestInteractionElements()
        {
            var cassette = TestUtils.GetCassette("test_interaction_elements");
            cassette.Erase(); // Erase cassette before recording

            var client = HttpClients.NewHttpClient(cassette, Mode.Record);
            var fakeDataService = new FakeDataService(client);

            // Most elements of a VCR request are black-boxed, so we can't test them here.
            // Instead, we can get the recreated HttpResponseMessage and check the details.
            var response = await fakeDataService.GetPostsRawResponse();
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task TestCensors()
        {
            var cassette = TestUtils.GetCassette("test_censors");
            cassette.Erase(); // Erase cassette before recording

            // set up advanced settings
            const string censorString = "censored-by-test";
            var advancedSettings = new AdvancedSettings
            {
                Censors = new Censors(censorString).HideHeader("Date")
            };

            // record cassette with advanced settings first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record, advancedSettings);
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetPostsRawResponse();

            // now replay cassette
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            fakeDataService = new FakeDataService(client);
            var response = await fakeDataService.GetPostsRawResponse();

            // check that the replayed response contains the censored header
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Headers.Contains("Date"));
            var censoredHeader = response.Headers.GetValues("Date").FirstOrDefault();
            Assert.IsNotNull(censoredHeader);
            Assert.AreEqual(censoredHeader, censorString);
        }

        [TestMethod]
        public async Task TestMatchSettings()
        {
            var cassette = TestUtils.GetCassette("test_match_settings");
            cassette.Erase(); // Erase cassette before recording

            // record cassette first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record);
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetPostsRawResponse();

            // replay cassette with default match rules, should find a match
            client = HttpClients.NewHttpClient(cassette, Mode.Replay);
            client.DefaultRequestHeaders.Add("X-Custom-Header", "custom-value"); // add custom header to request, shouldn't matter when matching by default rules
            fakeDataService = new FakeDataService(client);
            var response = await fakeDataService.GetPostsRawResponse();
            Assert.IsNotNull(response);

            // replay cassette with custom match rules, should not find a match because request is different (throw exception)
            var advancedSettings = new AdvancedSettings
            {
                MatchRules = new MatchRules().ByEverything()
            };
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            client.DefaultRequestHeaders.Add("X-Custom-Header", "custom-value"); // add custom header to request, causing a match failure when matching by everything
            fakeDataService = new FakeDataService(client);
            await Assert.ThrowsExceptionAsync<VCRException>(async () => await fakeDataService.GetPostsRawResponse());
        }

        [TestMethod]
        public async Task TestDelay()
        {
            var cassette = TestUtils.GetCassette("test_delay");
            cassette.Erase(); // Erase cassette before recording

            // record cassette first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record);
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetPostsRawResponse();

            // baseline - how much time does it take to replay the cassette?
            client = HttpClients.NewHttpClient(cassette, Mode.Replay);
            fakeDataService = new FakeDataService(client);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var response = await fakeDataService.GetPostsRawResponse();
            stopwatch.Stop();

            // confirm the normal replay worked, note time
            Assert.IsNotNull(response);
            var normalReplayTime = (int)stopwatch.ElapsedMilliseconds;

            // set up advanced settings
            var delay = normalReplayTime + 3000; // add 3 seconds to the normal replay time, for good measure
            var advancedSettings = new AdvancedSettings
            {
                ManualDelay = delay
            };
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            fakeDataService = new FakeDataService(client);

            // time replay request
            stopwatch = new Stopwatch();
            stopwatch.Start();
            response = await fakeDataService.GetPostsRawResponse();
            stopwatch.Stop();

            // check that the delay was respected
            Assert.IsNotNull(response);
            Assert.IsTrue((int)stopwatch.ElapsedMilliseconds >= delay);
        }
    }
}
