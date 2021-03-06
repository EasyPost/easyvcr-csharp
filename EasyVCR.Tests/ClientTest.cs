using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyVCR.Tests
{
    [TestClass]
    public class ClientTest
    {
        [TestMethod]
        public async Task TestAutoMode()
        {
            var cassette = TestUtils.GetCassette("test_auto_mode");
            cassette.Erase(); // Erase cassette before recording

            // in replay mode, if cassette is empty, should throw an exception
            await Assert.ThrowsExceptionAsync<VCRException>(async () => await GetIPAddressDataRequest(cassette, Mode.Replay));
            Assert.IsTrue(cassette.NumInteractions == 0); // Make sure cassette is still empty

            // in auto mode, if cassette is empty, should make and record a real request
            var summary = await GetIPAddressDataRequest(cassette, Mode.Auto);
            Assert.IsNotNull(summary);
            Assert.IsNotNull(summary.IPAddress);
            Assert.IsTrue(cassette.NumInteractions > 0); // Make sure cassette is no longer empty
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
                Censors = new Censors(censorString).CensorHeadersByKeys(new List<string> { "Date" })
            };

            // record cassette with advanced settings first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record, advancedSettings);
            var fakeDataService = new FakeJsonDataService(client);
            var _ = await fakeDataService.GetIPAddressDataRawResponse();

            // now replay cassette
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            fakeDataService = new FakeJsonDataService(client);
            var response = await fakeDataService.GetIPAddressDataRawResponse();

            // check that the replayed response contains the censored header
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Headers.Contains("Date"));
            var censoredHeader = response.Headers.GetValues("Date").FirstOrDefault();
            Assert.IsNotNull(censoredHeader);
            Assert.AreEqual(censoredHeader, censorString);
        }

        [TestMethod]
        public void TestClient()
        {
            var client = TestUtils.GetSimpleClient("test_client", Mode.Bypass);

            Assert.IsNotNull(client);
        }

        [TestMethod]
        public async Task TestDefaultRequestMatching()
        {
            // test that match by method and url works
            var cassette = TestUtils.GetCassette("test_default_request_matching");
            cassette.Erase(); // Erase cassette before recording

            const string postUrl = "https://google.com";
            var postBody = new StringContent("{\"key\":\"value\"}");

            // record cassette first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record, new AdvancedSettings
            {
                MatchRules = MatchRules.Default // doesn't really matter for initial record
            });
            var response = await client.PostAsync(postUrl, postBody);

            // check that the request body was not matched (should be a live call)
            Assert.IsNotNull(response);
            Assert.IsFalse(Utilities.ResponseCameFromRecording(response));

            // replay cassette
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, new AdvancedSettings
            {
                MatchRules = MatchRules.Default
            });
            response = await client.PostAsync(postUrl, postBody);

            // check that the request body was matched
            Assert.IsNotNull(response);
            Assert.IsTrue(Utilities.ResponseCameFromRecording(response));
        }

        [TestMethod]
        public async Task TestDelay()
        {
            var cassette = TestUtils.GetCassette("test_delay");
            cassette.Erase(); // Erase cassette before recording

            // record cassette first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record);
            var fakeDataService = new FakeJsonDataService(client);
            var _ = await fakeDataService.GetIPAddressDataRawResponse();

            // baseline - how much time does it take to replay the cassette?
            client = HttpClients.NewHttpClient(cassette, Mode.Replay);
            fakeDataService = new FakeJsonDataService(client);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var response = await fakeDataService.GetIPAddressDataRawResponse();
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
            fakeDataService = new FakeJsonDataService(client);

            // time replay request
            stopwatch = new Stopwatch();
            stopwatch.Start();
            response = await fakeDataService.GetIPAddressDataRawResponse();
            stopwatch.Stop();

            // check that the delay was respected
            Assert.IsNotNull(response);
            Assert.IsTrue((int)stopwatch.ElapsedMilliseconds >= delay);
        }

        [TestMethod]
        public async Task TestErase()
        {
            var cassette = TestUtils.GetCassette("test_erase");

            // record something to the cassette
            var _ = await GetIPAddressDataRequest(cassette, Mode.Record);
            Assert.IsTrue(cassette.NumInteractions > 0);

            // erase the cassette
            cassette.Erase();
            Assert.IsTrue(cassette.NumInteractions == 0);
        }

        [TestMethod]
        public async Task TestEraseAndPlayback()
        {
            var cassette = TestUtils.GetCassette("test_erase_and_record");
            cassette.Erase(); // Erase cassette before recording

            // cassette is empty, so replaying should throw an exception
            await Assert.ThrowsExceptionAsync<VCRException>(async () => await GetIPAddressDataRequest(cassette, Mode.Replay));
        }

        [TestMethod]
        public async Task TestEraseAndRecord()
        {
            var cassette = TestUtils.GetCassette("test_erase_and_record");
            cassette.Erase(); // Erase cassette before recording

            var summary = await GetIPAddressDataRequest(cassette, Mode.Record);

            Assert.IsNotNull(summary);
            Assert.IsNotNull(summary.IPAddress);
            Assert.IsTrue(cassette.NumInteractions > 0); // Make sure cassette is not empty
        }

        [TestMethod]
        public async Task TestExpirationSettings()
        {
            var cassette = TestUtils.GetCassette("test_expiration_settings");
            cassette.Erase(); // Erase cassette before recording

            // record cassette first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record);
            var fakeDataService = new FakeJsonDataService(client);
            await fakeDataService.GetIPAddressDataRawResponse();

            // replay cassette with default expiration rules, should find a match
            client = HttpClients.NewHttpClient(cassette, Mode.Replay);
            fakeDataService = new FakeJsonDataService(client);
            var response = await fakeDataService.GetIPAddressDataRawResponse();
            Assert.IsNotNull(response);

            // replay cassette with custom expiration rules, should not find a match because recording is expired (throw exception)
            var advancedSettings = new AdvancedSettings
            {
                ValidTimeFrame = TimeFrame.Never,
                WhenExpired = ExpirationActions.ThrowException // throw exception when in replay mode
            };
            Task.Delay(TimeSpan.FromSeconds(1)).Wait(); // Allow 1 second to lapse to ensure recording is now "expired"
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            fakeDataService = new FakeJsonDataService(client);
            await Assert.ThrowsExceptionAsync<VCRException>(async () => await fakeDataService.GetIPAddressDataRawResponse());

            // replay cassette with bad expiration rules, should throw an exception because settings are bad
            advancedSettings = new AdvancedSettings
            {
                ValidTimeFrame = TimeFrame.Never,
                WhenExpired = ExpirationActions.RecordAgain // invalid settings for replay mode, should throw exception
            };
            await Assert.ThrowsExceptionAsync<VCRException>(() => Task.FromResult(HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings)));
        }

        [TestMethod]
        public void TestFakeDataServiceClient()
        {
            var client = TestUtils.GetSimpleClient("test_fake_data_service_client", Mode.Bypass);

            var fakeDataService = new FakeJsonDataService(client);
            Assert.IsNotNull(fakeDataService.Client);
        }

        [TestMethod]
        public async Task TestIgnoreElementsFailMatch()
        {
            var cassette = TestUtils.GetCassette("test_ignore_elements_fail_match");
            cassette.Erase(); // Erase cassette before recording

            var bodyData1 = new StringContent("{\"name\": \"Jack Sparrow\",\n    \"company\": \"EasyPost\"}");
            var bodyData2 = new StringContent("{\"name\": \"Different Name\",\n    \"company\": \"EasyPost\"}");

            // record baseline request first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record);
            var _ = await client.PostAsync(FakeDataService.GetIPAddressDataUrl("json"), bodyData1);

            // try to replay the request with different body data
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, new AdvancedSettings
            {
                MatchRules = new MatchRules().ByBody().ByMethod().ByFullUrl()
            });

            // should fail since we're strictly in replay mode and there's no exact match
            await Assert.ThrowsExceptionAsync<VCRException>(async () => await client.PostAsync(FakeDataService.GetIPAddressDataUrl("json"), bodyData2));
        }

        [TestMethod]
        public async Task TestIgnoreElementsPassMatch()
        {
            var cassette = TestUtils.GetCassette("test_ignore_elements_pass_match");
            cassette.Erase(); // Erase cassette before recording

            var bodyData1 = new StringContent("{\"name\": \"Jack Sparrow\",\n    \"company\": \"EasyPost\"}");
            var bodyData2 = new StringContent("{\"name\": \"Different Name\",\n    \"company\": \"EasyPost\"}");

            // record baseline request first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record);
            var _ = await client.PostAsync(FakeDataService.GetIPAddressDataUrl("json"), bodyData1);

            // try to replay the request with different body data, but ignoring the differences
            var ignoreElements = new List<CensorElement>
            {
                new CensorElement("name", false)
            };
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, new AdvancedSettings
            {
                MatchRules = new MatchRules().ByBody(ignoreElements).ByMethod().ByFullUrl()
            });

            // should succeed since we're ignoring the differences
            var response = await client.PostAsync(FakeDataService.GetIPAddressDataUrl("json"), bodyData2);
            Assert.IsNotNull(response);
            Assert.IsTrue(Utilities.ResponseCameFromRecording(response));
        }


        [TestMethod]
        public async Task TestInteractionElements()
        {
            var cassette = TestUtils.GetCassette("test_interaction_elements");
            cassette.Erase(); // Erase cassette before recording

            var client = HttpClients.NewHttpClient(cassette, Mode.Record);
            var fakeDataService = new FakeJsonDataService(client);

            // Most elements of a VCR request are black-boxed, so we can't test them here.
            // Instead, we can get the recreated HttpResponseMessage and check the details.
            var response = await fakeDataService.GetIPAddressDataRawResponse();
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task TestMatchSettings()
        {
            var cassette = TestUtils.GetCassette("test_match_settings");
            cassette.Erase(); // Erase cassette before recording

            // record cassette first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record);
            var fakeDataService = new FakeJsonDataService(client);
            var _ = await fakeDataService.GetIPAddressDataRawResponse();

            // replay cassette with default match rules, should find a match
            client = HttpClients.NewHttpClient(cassette, Mode.Replay);
            client.DefaultRequestHeaders.Add("X-Custom-Header", "custom-value"); // add custom header to request, shouldn't matter when matching by default rules
            fakeDataService = new FakeJsonDataService(client);
            var response = await fakeDataService.GetIPAddressDataRawResponse();
            Assert.IsNotNull(response);

            // replay cassette with custom match rules, should not find a match because request is different (throw exception)
            var advancedSettings = new AdvancedSettings
            {
                MatchRules = new MatchRules().ByEverything()
            };
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            client.DefaultRequestHeaders.Add("X-Custom-Header", "custom-value"); // add custom header to request, causing a match failure when matching by everything
            fakeDataService = new FakeJsonDataService(client);
            await Assert.ThrowsExceptionAsync<VCRException>(async () => await fakeDataService.GetIPAddressDataRawResponse());
        }

        [TestMethod]
        public async Task TestNestedCensoring()
        {
            var cassette = TestUtils.GetCassette("test_nested_censoring");
            cassette.Erase(); // Erase cassette before recording

            const string postUrl = "https://google.com";
            var postBody = new StringContent("{\r\n  \"array\": [\r\n    \"array_1\",\r\n    \"array_2\",\r\n    \"array_3\"\r\n  ],\r\n  \"dict\": {\r\n    \"nested_array\": [\r\n      \"nested_array_1\",\r\n      \"nested_array_2\",\r\n      \"nested_array_3\"\r\n    ],\r\n    \"nested_dict\": {\r\n      \"nested_dict_1\": {\r\n        \"nested_dict_1_1\": {\r\n          \"nested_dict_1_1_1\": \"nested_dict_1_1_1_value\"\r\n        }\r\n      },\r\n      \"nested_dict_2\": {\r\n        \"nested_dict_2_1\": \"nested_dict_2_1_value\",\r\n        \"nested_dict_2_2\": \"nested_dict_2_2_value\"\r\n      }\r\n    },\r\n    \"dict_1\": \"dict_1_value\",\r\n    \"null_key\": null\r\n  }\r\n}");
            // set up advanced settings
            const string censorString = "censored-by-test";
            var censors = new Censors(censorString);
            censors.CensorBodyElementsByKeys(new List<string> { "nested_dict_1_1_1", "nested_dict_2_2", "nested_array", "null_key" });
            var advancedSettings = new AdvancedSettings
            {
                Censors = censors
            };

            // record cassette
            var client = HttpClients.NewHttpClient(cassette, Mode.Record, advancedSettings);
            var _ = await client.PostAsync(postUrl, postBody);

            // NOTE: Have to manually check the cassette
        }

        [TestMethod]
        public async Task TestStrictRequestMatching()
        {
            // test that match by method, url and body works
            var cassette = TestUtils.GetCassette("test_strict_request_matching");
            cassette.Erase(); // Erase cassette before recording

            const string postUrl = "https://google.com";
            var postBody = new StringContent("{\n  \"address\": {\n    \"name\": \"Jack Sparrow\",\n    \"company\": \"EasyPost\",\n    \"street1\": \"388 Townsend St\",\n    \"street2\": \"Apt 20\",\n    \"city\": \"San Francisco\",\n    \"state\": \"CA\",\n    \"zip\": \"94107\",\n    \"country\": \"US\",\n    \"phone\": \"5555555555\"\n  }\n}");

            // record cassette first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record, new AdvancedSettings
            {
                MatchRules = MatchRules.DefaultStrict // doesn't really matter for initial record
            });
            var response = await client.PostAsync(postUrl, postBody);

            // check that the request body was not matched (should be a live call)
            Assert.IsNotNull(response);
            Assert.IsFalse(Utilities.ResponseCameFromRecording(response));

            // replay cassette
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, new AdvancedSettings
            {
                MatchRules = MatchRules.DefaultStrict
            });
            response = await client.PostAsync(postUrl, postBody);

            // check that the request body was matched
            Assert.IsNotNull(response);
            Assert.IsTrue(Utilities.ResponseCameFromRecording(response));
        }

        [Ignore]
        [TestMethod]
        public async Task TestXmlCensoring()
        {
            var cassette = TestUtils.GetCassette("test_xml_censors");
            cassette.Erase(); // Erase cassette before recording

            // set up advanced settings
            const string censorString = "censored-by-test";
            var advancedSettings = new AdvancedSettings
            {
                Censors = new Censors(censorString).CensorBodyElementsByKeys(new List<string> { "Table" })
            };

            // record cassette with advanced settings first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record, advancedSettings);
            var fakeDataService = new FakeXmlDataService(client);
            var _ = await fakeDataService.GetIPAddressDataRawResponse();

            // now replay cassette
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            fakeDataService = new FakeXmlDataService(client);
            _ = await fakeDataService.GetIPAddressDataRawResponse();

            // TODO: Test is failing because the response is not being censored.
            // have to manually check cassette for the censored string in the response body
        }

        private static async Task<IPAddressData?> GetIPAddressDataRequest(Cassette cassette, Mode mode)
        {
            var client = HttpClients.NewHttpClient(cassette, mode, new AdvancedSettings
            {
                MatchRules = MatchRules.DefaultStrict
            });

            var fakeDataService = new FakeJsonDataService(client);

            return await fakeDataService.GetIPAddressData();
        }
    }
}
