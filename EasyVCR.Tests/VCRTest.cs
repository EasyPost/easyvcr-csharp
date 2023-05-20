using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyVCR.Tests
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class VCRTest
    {
        [TestMethod]
        public async Task TestAdvancedSettings()
        {
            // we can assume that, if one test of advanced settings works for the VCR,
            // that the advanced settings are being properly passed to the cassette
            // refer to ClientTest.cs for individual per-settings tests

            const string censorString = "censored-by-test";
            var headerCensors = new List<KeyCensorElement>
            {
                new("Date", true),
            };
            var advancedSettings = new AdvancedSettings
            {
                Censors = new Censors(censorString).CensorHeaders(headerCensors),
            };

            var vcr = new VCR(advancedSettings);

            // test that the advanced settings are applied inside the VCR
            Assert.AreEqual(vcr.AdvancedSettings, advancedSettings);

            // test that the advanced settings are passed to the cassette by checking if censor is applied
            var cassette = TestUtils.GetCassette("test_vcr_advanced_settings");
            vcr.Insert(cassette);
            vcr.Erase(); // erase before recording

            // record first
            vcr.Record();
            var client = vcr.Client;
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetJsonDataRawResponse();

            // now replay and confirm that the censor is applied
            vcr.Replay();
            // changing the VCR settings won't affect a client after it's been grabbed from the VCR
            // so, we need to re-grab the VCR client and re-create the FakeDataService
            client = vcr.Client;
            fakeDataService = new FakeDataService(client);
            var response = await fakeDataService.GetJsonDataRawResponse();
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Headers.Contains("Date"));
            var censoredHeader = response.Headers.GetValues("Date").FirstOrDefault();
            Assert.IsNotNull(censoredHeader);
            Assert.AreEqual(censoredHeader, censorString);
        }

        [TestMethod]
        public Task TestCassetteName()
        {
            const string cassetteName = "test_vcr_cassette_name";
            var cassette = TestUtils.GetCassette(cassetteName);
            var vcr = TestUtils.GetSimpleVCR(Mode.Bypass);
            vcr.Insert(cassette);

            // make sure the cassette name is set correctly
            Assert.AreEqual(cassetteName, vcr.CassetteName);
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task TestCassetteSwap()
        {
            const string cassette1Name = "test_vcr_cassette_swap_1";
            const string cassette2Name = "test_vcr_cassette_swap_2";

            var vcr = new VCR();

            var cassette = TestUtils.GetCassette(cassette1Name);
            vcr.Insert(cassette);
            Assert.AreEqual(cassette1Name, vcr.CassetteName);

            vcr.Eject();
            Assert.AreEqual(null, vcr.CassetteName);

            cassette = TestUtils.GetCassette(cassette2Name);
            vcr.Insert(cassette);
            Assert.AreEqual(cassette2Name, vcr.CassetteName);
            return Task.CompletedTask;
        }

        [TestMethod]
        public void TestClient()
        {
            var cassette = TestUtils.GetCassette("test_vcr_client");
            var vcr = TestUtils.GetSimpleVCR(Mode.Bypass);
            vcr.Insert(cassette);

            Assert.IsNotNull(vcr.Client);
        }

        [TestMethod]
        public void TestClientHandOff()
        {
            var cassette = TestUtils.GetCassette("test_vcr_mode_hand_off");
            var vcr = TestUtils.GetSimpleVCR(Mode.Bypass);
            vcr.Insert(cassette);

            // test that we can still control the VCR even after it's been handed off to the service using it
            var fakeDataService = new FakeDataService(vcr);
            // Client should come from VCR, which has a client because it has a cassette.
            Assert.IsNotNull(fakeDataService.Client);

            vcr.Eject();
            // Client should be null because the VCR's cassette has been ejected.
            Assert.ThrowsException<InvalidOperationException>(() => fakeDataService.Client);
        }

        [TestMethod]
        public void TestClientNoCassette()
        {
            var vcr = TestUtils.GetSimpleVCR(Mode.Bypass);
            // Client should be null because the VCR has no cassette.
            Assert.ThrowsException<InvalidOperationException>(() => vcr.Client);
        }

        [TestMethod]
        public void TestEjectCassette()
        {
            var cassette = TestUtils.GetCassette("test_vcr_eject_cassette");
            var vcr = TestUtils.GetSimpleVCR(Mode.Bypass);
            vcr.Insert(cassette);
            Assert.IsNotNull(vcr.CassetteName);
            vcr.Eject();
            Assert.IsNull(vcr.CassetteName);
        }

        [TestMethod]
        public async Task TestErase()
        {
            var cassette = TestUtils.GetCassette("test_vcr_eject_cassette");
            cassette.Erase(); // make sure the cassette is empty
            var vcr = TestUtils.GetSimpleVCR(Mode.Record);
            vcr.Insert(cassette);

            // record a request to a cassette
            var fakeDataService = new FakeDataService(vcr);
            var responseBody = await fakeDataService.GetJsonData();
            Assert.IsNotNull(responseBody);
            Assert.IsTrue(cassette.NumInteractions > 0);

            // erase the cassette
            vcr.Erase();
            Assert.IsTrue(cassette.NumInteractions == 0);
        }

        [TestMethod]
        public void TestInsertCassette()
        {
            var cassette = TestUtils.GetCassette("test_vcr_insert_cassette");
            var vcr = TestUtils.GetSimpleVCR(Mode.Bypass);
            vcr.Insert(cassette);
            Assert.AreEqual(cassette.Name, vcr.CassetteName);
        }

        [TestMethod]
        public void TestMode()
        {
            TestUtils.GetCassette("test_vcr_mode");
            var vcr = TestUtils.GetSimpleVCR(Mode.Bypass);
            Assert.AreEqual(Mode.Bypass, vcr.Mode);
            vcr.Record();
            Assert.AreEqual(vcr.Mode, Mode.Record);
            vcr.Replay();
            Assert.AreEqual(vcr.Mode, Mode.Replay);
            vcr.Pause();
            Assert.AreEqual(vcr.Mode, Mode.Bypass);
            vcr.RecordIfNeeded();
            Assert.AreEqual(vcr.Mode, Mode.Auto);
        }

        [TestMethod]
        public async Task TestRecord()
        {
            var cassette = TestUtils.GetCassette("test_vcr_record");
            var vcr = TestUtils.GetSimpleVCR(Mode.Record);
            vcr.Insert(cassette);
            var fakeDataService = new FakeDataService(vcr);

            var responseBody = await fakeDataService.GetJsonData();
            Assert.IsNotNull(responseBody);
            Assert.IsTrue(cassette.NumInteractions > 0);
        }

        [TestMethod]
        public async Task TestReplay()
        {
            var cassette = TestUtils.GetCassette("test_vcr_replay");
            var vcr = TestUtils.GetSimpleVCR(Mode.Record);
            vcr.Insert(cassette);
            var fakeDataService = new FakeDataService(vcr);

            // record first
            var _ = await fakeDataService.GetJsonDataRawResponse();
            Assert.IsTrue(cassette.NumInteractions > 0); // make sure we recorded something

            // now replay
            vcr.Replay();
            var responseBody = await fakeDataService.GetJsonData();
            Assert.IsNotNull(responseBody);

            // double check by erasing the cassette and trying to replay
            vcr.Erase();
            // should throw an exception because there's no matching interaction now
            await Assert.ThrowsExceptionAsync<VCRException>(async () => await fakeDataService.GetJsonDataRawResponse());
        }

        [TestMethod]
        public async Task TestRequest()
        {
            var cassette = TestUtils.GetCassette("test_vcr_record");
            var vcr = TestUtils.GetSimpleVCR(Mode.Bypass);
            vcr.Insert(cassette);
            var fakeDataService = new FakeDataService(vcr);

            var responseBody = await fakeDataService.GetJsonData();
            Assert.IsNotNull(responseBody);
        }
    }
}
