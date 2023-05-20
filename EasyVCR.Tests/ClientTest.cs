using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using EasyVCR.Handlers;
using EasyVCR.RequestElements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
// ReSharper disable InconsistentNaming

namespace EasyVCR.Tests
{
    [TestClass]
    public class ClientTest
    {
        [TestMethod]
        public void TestClientCasting()
        {
            var client = HttpClients.NewHttpClient("cassette_folder", "cassette_name", Mode.Auto);

            // check that client is of type EasyVcrHttpClient
            Assert.IsInstanceOfType(client, typeof(EasyVCRHttpClient));

            // check if the client is castable to HttpClient
            Assert.IsInstanceOfType(client, typeof(HttpClient));

            // cast to HttpClient and recheck type
            var httpClient = (HttpClient)client;
            Assert.IsInstanceOfType(httpClient, typeof(HttpClient));
        }

        [TestMethod]
        public async Task TestClientClone()
        {
            var client = HttpClients.NewHttpClient("cassettes", "test_client_clone", Mode.Record);
            var clonedClient = client.Clone();

            // check that the two clients are equal
            Assert.AreEqual(client, clonedClient);

            // technically, the equality of an EasyVCRHttpClient is comparing the equality of the internal VcrHandler, so let's explicitly verify that.
            var handler = client.VcrHandler;
            var clonedHandler = clonedClient.VcrHandler;
            Assert.AreEqual(handler, clonedHandler);

            // lock the original client into a request (internally, RestClient will pass/lock the options to the HttpClient, which is what causes this issue)
            var options = new RestClientOptions
            {
                MaxTimeout = 60000,
                UserAgent = "EasyVCR Test Client",
                BaseUrl = new Uri("https://httpbin.org"),
            };

            var restClient = new RestClient(client, options);
            var request = new RestRequest("https://www.google.com");
            var _ = await restClient.ExecuteAsync(request);

            // now, if we try to reuse the client in another RestClient, it will throw an exception that the HttpClient is already in use
            Assert.ThrowsException<InvalidOperationException>(() => new RestClient(client, options));

            // even if we try with a different set of options
            var options2 = new RestClientOptions
            {
                MaxTimeout = 30000,
                UserAgent = "EasyVCR Test Client 2",
                BaseUrl = new Uri("https://example.com"),
            };
            Assert.ThrowsException<InvalidOperationException>(() => new RestClient(client, options2));

            // if we use the cloned client, it will work
            var restClient3 = new RestClient(clonedClient, options);
            var request3 = new RestRequest("https://www.google.com");
            _ = await restClient3.ExecuteAsync(request3);

            // side-note, you CAN re-use the original client if you don't have any options (it's the RestClientOptions that's causing this issue)
            var restClient4 = new RestClient(client);
            var request4 = new RestRequest("https://www.google.com");
            _ = await restClient4.ExecuteAsync(request4);
        }

        [TestMethod]
        public async Task TestAutoMode()
        {
            var cassette = TestUtils.GetCassette("test_auto_mode");
            cassette.Erase(); // Erase cassette before recording

            // in replay mode, if cassette is empty, should throw an exception
            await Assert.ThrowsExceptionAsync<VCRException>(async () => await GetJsonDataRequest(cassette, Mode.Replay));
            Assert.IsTrue(cassette.NumInteractions == 0); // Make sure cassette is still empty

            // in auto mode, if cassette is empty, should make and record a real request
            var responseBody = await GetJsonDataRequest(cassette, Mode.Auto);
            Assert.IsNotNull(responseBody);
            Assert.IsTrue(cassette.NumInteractions > 0); // Make sure cassette is no longer empty
        }

        [TestMethod]
        public async Task TestCensors()
        {
            var cassette = TestUtils.GetCassette("test_censors");
            cassette.Erase(); // Erase cassette before recording

            // set up advanced settings
            const string censorString = "censored-by-test";
            var headerCensors = new List<KeyCensorElement> { new KeyCensorElement("Date", false) };
            var advancedSettings = new AdvancedSettings
            {
                Censors = new Censors(censorString).CensorHeaders(headerCensors),
            };

            // record cassette with advanced settings first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record, advancedSettings);
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetJsonDataRawResponse();

            // now replay cassette
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            fakeDataService = new FakeDataService(client);
            var response = await fakeDataService.GetJsonDataRawResponse();

            // check that the replayed response contains the censored header
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Headers.Contains("Date"));
            var censoredHeader = response.Headers.GetValues("Date").FirstOrDefault();
            Assert.IsNotNull(censoredHeader);
            Assert.AreEqual(censoredHeader, censorString);
        }

        /// <summary>
        ///     Test that the RegexCensor works as expected.
        ///     NOTE: This test does not currently programmatically verify that the RegexCensor is working as expected.
        ///     Instead, it relies on the developer to manually verify that the cassette file contains the expected censored values.
        /// </summary>
        [TestMethod]
        public async Task TestRegexCensors()
        {
            var cassette = TestUtils.GetCassette("test_regex_censors");
            cassette.Erase(); // Erase cassette before recording

            // set up regex pattern
            var url = new Uri(FakeDataService.JsonDataUrl);
            var domain = url.Host;
            var regexPattern = domain;

            // set up advanced settings
            const string censorString = "censored-by-test";
            var pathCensors = new List<RegexCensorElement> { new RegexCensorElement(regexPattern, false) };
            var advancedSettings = new AdvancedSettings
            {
                Censors = new Censors(censorString).CensorPathElements(pathCensors),
            };

            // record cassette with advanced settings
            var client = HttpClients.NewHttpClient(cassette, Mode.Record, advancedSettings);
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetJsonDataRawResponse();

            // verify that censoring does not interfere with replay
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            fakeDataService = new FakeDataService(client);
            var response = await fakeDataService.GetJsonDataRawResponse();
            Assert.IsNotNull(response);
        }

        /// <summary>
        ///     This test confirms that the <see cref="EasyVCRHttpClient"/> is constructed correctly.
        /// </summary>
        [TestMethod]
        public void TestClient()
        {
            var client = TestUtils.GetSimpleClient("test_client", Mode.Bypass);

            Assert.IsNotNull(client);
        }

        /// <summary>
        ///     This test confirms that users can construct their own <see cref="VCRHandler"/> for their own needs,
        ///     if they don't want to use the pre-configured <see cref="EasyVCRHttpClient"/>.
        /// </summary>
        [TestMethod]
        public void TestVCRHandler()
        {
            var cassette = TestUtils.GetCassette("test_client_handler");
            const Mode mode = Mode.Bypass;

            var vcrHandler = VCRHandler.NewVCRHandler(cassette, mode);

            Assert.IsNotNull(vcrHandler);
            Assert.IsNull(vcrHandler.InnerHandler);
        }

        /// <summary>
        ///     This test confirms that users can construct their own <see cref="VCRHandler"/> for their own needs,
        ///     with its own inner handler, if they don't want to use the pre-configured <see cref="EasyVCRHttpClient"/>.
        /// </summary>
        [TestMethod]
        public void TestVCRHandlerWithInnerHandler()
        {
            var cassette = TestUtils.GetCassette("test_client_handler_with_inner_handler");
            const Mode mode = Mode.Bypass;

            var vcrHandler = VCRHandler.NewVCRHandler(cassette, mode, innerHandler: new HttpClientHandler());

            Assert.IsNotNull(vcrHandler);
            Assert.IsNotNull(vcrHandler.InnerHandler);
        }

        [TestMethod]
        public async Task TestDefaultRequestMatching()
        {
            // test that match by method and url works
            var cassette = TestUtils.GetCassette("test_default_request_matching");
            cassette.Erase(); // Erase cassette before recording

            const string postUrl = "http://httpbin.org/post";
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
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetJsonDataRawResponse();

            // baseline - how much time does it take to replay the cassette?
            client = HttpClients.NewHttpClient(cassette, Mode.Replay);
            fakeDataService = new FakeDataService(client);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var response = await fakeDataService.GetJsonDataRawResponse();
            stopwatch.Stop();

            // confirm the normal replay worked, note time
            Assert.IsNotNull(response);
            var normalReplayTime = Math.Max(0, (int)stopwatch.ElapsedMilliseconds); // sometimes stopwatch returns a negative number, let's avoid that

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
            response = await fakeDataService.GetJsonDataRawResponse();
            stopwatch.Stop();

            // check that the delay was respected (within margin of error)
            Assert.IsNotNull(response);
            var delay95Percentile = (int)(delay * 0.95); // 5% tolerance
            Assert.IsTrue((int)stopwatch.ElapsedMilliseconds >= delay95Percentile);
        }

        [TestMethod]
        public async Task TestErase()
        {
            var cassette = TestUtils.GetCassette("test_erase");

            // record something to the cassette
            var _ = await GetJsonDataRequest(cassette, Mode.Record);
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
            await Assert.ThrowsExceptionAsync<VCRException>(async () => await GetJsonDataRequest(cassette, Mode.Replay));
        }

        [TestMethod]
        public async Task TestEraseAndRecord()
        {
            var cassette = TestUtils.GetCassette("test_erase_and_record");
            cassette.Erase(); // Erase cassette before recording

            var responseBody = await GetJsonDataRequest(cassette, Mode.Record);

            Assert.IsNotNull(responseBody);
            Assert.IsTrue(cassette.NumInteractions > 0); // Make sure cassette is not empty
        }

        [TestMethod]
        public async Task TestExpirationSettings()
        {
            var cassette = TestUtils.GetCassette("test_expiration_settings");
            cassette.Erase(); // Erase cassette before recording

            // record cassette first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record);
            var fakeDataService = new FakeDataService(client);
            await fakeDataService.GetJsonDataRawResponse();

            // replay cassette with default expiration rules, should find a match
            client = HttpClients.NewHttpClient(cassette, Mode.Replay);
            fakeDataService = new FakeDataService(client);
            var response = await fakeDataService.GetJsonDataRawResponse();
            Assert.IsNotNull(response);

            // replay cassette with custom expiration rules, should not find a match because recording is expired (throw exception)
            var advancedSettings = new AdvancedSettings
            {
                ValidTimeFrame = TimeFrame.Never,
                WhenExpired = ExpirationActions.ThrowException // throw exception when in replay mode
            };
            Task.Delay(TimeSpan.FromSeconds(1)).Wait(); // Allow 1 second to lapse to ensure recording is now "expired"
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            fakeDataService = new FakeDataService(client);
            await Assert.ThrowsExceptionAsync<VCRException>(async () => await fakeDataService.GetJsonDataRawResponse());

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

            var fakeDataService = new FakeDataService(client);
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
            var _ = await client.PostAsync(FakeDataService.JsonDataUrl, bodyData1);

            // try to replay the request with different body data
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, new AdvancedSettings
            {
                MatchRules = new MatchRules().ByBody().ByMethod().ByFullUrl()
            });

            // should fail since we're strictly in replay mode and there's no exact match
            await Assert.ThrowsExceptionAsync<VCRException>(async () => await client.PostAsync(FakeDataService.JsonDataUrl, bodyData2));
        }

        [TestMethod]
        public async Task TestCustomMatchRule()
        {
            var cassette = TestUtils.GetCassette("test_custom_match_rule");
            cassette.Erase(); // Erase cassette before recording

            var bodyData1 = new StringContent("{\"name\": \"Jack Sparrow\",\n    \"company\": \"EasyPost\"}");
            var bodyData2 = new StringContent("{\"name\": \"Different Name\",\n    \"company\": \"EasyPost\"}");

            // record baseline request first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record);
            var _ = await client.PostAsync(FakeDataService.JsonDataUrl, bodyData1);

            // try to replay the request with no custom match rule
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, new AdvancedSettings()
            {
                MatchRules = new MatchRules(),
            });

            // should pass since it passes the default match rules
            await client.PostAsync(FakeDataService.JsonDataUrl, bodyData1);

            // try to replay the request with a custom match rule
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, new AdvancedSettings
            {
                MatchRules = new MatchRules().ByCustomRule(new Func<Request, Request, bool>((received, recorded) => false)), // always return false
            });

            // should fail since the custom match rule always returns false and there's never a match
            await Assert.ThrowsExceptionAsync<VCRException>(async () => await client.PostAsync(FakeDataService.JsonDataUrl, bodyData2));
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
            var _ = await client.PostAsync(FakeDataService.JsonDataUrl, bodyData1);

            // try to replay the request with different body data, but ignoring the differences
            var ignoreElements = new List<CensorElement>
            {
                new KeyCensorElement("name", false)
            };
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, new AdvancedSettings
            {
                MatchRules = new MatchRules().ByBody(ignoreElements).ByMethod().ByFullUrl()
            });

            // should succeed since we're ignoring the differences
            var response = await client.PostAsync(FakeDataService.JsonDataUrl, bodyData2);
            Assert.IsNotNull(response);
            Assert.IsTrue(Utilities.ResponseCameFromRecording(response));
        }

        [TestMethod]
        public async Task TestMatchNonJsonBody()
        {
            var cassette = TestUtils.GetCassette("test_match_non_json_body");
            cassette.Erase(); // Erase cassette before recording

            const string url = "http://httpbin.org/post";
            var postData = HttpUtility.UrlEncode($"param1=name&param2=age", Encoding.UTF8);
            var data = Encoding.UTF8.GetBytes(postData);
            var content = new ByteArrayContent(data);
            content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            // record baseline request first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record);
            var _ = await client.PostAsync(url, content);

            // try to replay the request with match by body enforcement
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, new AdvancedSettings
            {
                MatchRules = new MatchRules().ByBody()
            });
            var response = await client.PostAsync(url, content);
            Assert.IsNotNull(response);
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
            var response = await fakeDataService.GetJsonDataRawResponse();
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task TestMatchSettings()
        {
            var cassette = TestUtils.GetCassette("test_match_settings");
            cassette.Erase(); // Erase cassette before recording

            // record cassette first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record);
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetJsonDataRawResponse();

            // replay cassette with default match rules, should find a match
            client = HttpClients.NewHttpClient(cassette, Mode.Replay);
            client.DefaultRequestHeaders.Add("X-Custom-Header", "custom-value"); // add custom header to request, shouldn't matter when matching by default rules
            fakeDataService = new FakeDataService(client);
            var response = await fakeDataService.GetJsonDataRawResponse();
            Assert.IsNotNull(response);

            // replay cassette with custom match rules, should not find a match because request is different (throw exception)
            var advancedSettings = new AdvancedSettings
            {
                MatchRules = new MatchRules().ByEverything()
            };
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            client.DefaultRequestHeaders.Add("X-Custom-Header", "custom-value"); // add custom header to request, causing a match failure when matching by everything
            fakeDataService = new FakeDataService(client);
            await Assert.ThrowsExceptionAsync<VCRException>(async () => await fakeDataService.GetJsonDataRawResponse());
        }

        [TestMethod]
        public async Task TestHeadersFailMatch()
        {
            var cassette = TestUtils.GetCassette("test_headers_fail_match");
            cassette.Erase(); // Erase cassette before recording

            // record cassette first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record);
            client.DefaultRequestHeaders.Add("X-Custom-Header", "custom-value1");
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetJsonDataRawResponse();

            // replay cassette with default match rules, should find a match
            client = HttpClients.NewHttpClient(cassette, Mode.Replay);
            // no custom header to request, shouldn't matter when matching by default rules
            fakeDataService = new FakeDataService(client);
            var response = await fakeDataService.GetJsonDataRawResponse();
            Assert.IsNotNull(response);

            // replay cassette with custom match rules, should not find a match because header value is different (throw exception)
            var advancedSettings = new AdvancedSettings
            {
                MatchRules = new MatchRules().ByHeader("X-Custom-Header")
            };
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            client.DefaultRequestHeaders.Add("X-Custom-Header", "custom-value2"); // add header with different value to request
            fakeDataService = new FakeDataService(client);
            await Assert.ThrowsExceptionAsync<VCRException>(async () => await fakeDataService.GetJsonDataRawResponse());
        }

        [TestMethod]
        public async Task TestMissingHeaderFailMatch()
        {
            var cassette = TestUtils.GetCassette("test_missing_header_fail_match");
            cassette.Erase(); // Erase cassette before recording

            // record cassette first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record);
            client.DefaultRequestHeaders.Add("X-Custom-Header", "custom-value");
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetJsonDataRawResponse();

            // replay cassette with custom match rules, should not find a match because header is missing (throw exception)
            var advancedSettings = new AdvancedSettings
            {
                MatchRules = new MatchRules().ByHeader("X-Custom-Header")
            };
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            fakeDataService = new FakeDataService(client);
            await Assert.ThrowsExceptionAsync<VCRException>(async () => await fakeDataService.GetJsonDataRawResponse());
        }

        [TestMethod]
        public async Task TestHeadersPassMatch()
        {
            var cassette = TestUtils.GetCassette("test_headers_pass_match");
            cassette.Erase(); // Erase cassette before recording

            // record cassette first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record);
            client.DefaultRequestHeaders.Add("X-Custom-Header1", "custom-value1"); // add custom header to request
            client.DefaultRequestHeaders.Add("X-Custom-Header2", "custom-value2");
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetJsonDataRawResponse();

            // replay cassette with custom match rules
            var advancedSettings = new AdvancedSettings
            {
                MatchRules = new MatchRules().ByHeader("X-Custom-Header1").ByHeader("X-Custom-Header3")
            };
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            client.DefaultRequestHeaders.Add("X-Custom-Header1", "custom-value1");
            fakeDataService = new FakeDataService(client);
            var response = await fakeDataService.GetJsonDataRawResponse();

            // should succeed since X-Custom-Header1 header value equals
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task TestNestedCensoring()
        {
            var cassette = TestUtils.GetCassette("test_nested_censoring");
            cassette.Erase(); // Erase cassette before recording

            const string postUrl = "http://httpbin.org/post";
            var postBody = new StringContent("{\r\n  \"array\": [\r\n    \"array_1\",\r\n    \"array_2\",\r\n    \"array_3\"\r\n  ],\r\n  \"dict\": {\r\n    \"nested_array\": [\r\n      \"nested_array_1\",\r\n      \"nested_array_2\",\r\n      \"nested_array_3\"\r\n    ],\r\n    \"nested_dict\": {\r\n      \"nested_dict_1\": {\r\n        \"nested_dict_1_1\": {\r\n          \"nested_dict_1_1_1\": \"nested_dict_1_1_1_value\"\r\n        }\r\n      },\r\n      \"nested_dict_2\": {\r\n        \"nested_dict_2_1\": \"nested_dict_2_1_value\",\r\n        \"nested_dict_2_2\": \"nested_dict_2_2_value\"\r\n      }\r\n    },\r\n    \"dict_1\": \"dict_1_value\",\r\n    \"null_key\": null\r\n  }\r\n}");
            // set up advanced settings
            const string censorString = "censored-by-test";
            var censors = new Censors(censorString);
            var bodyCensors = new List<KeyCensorElement>
            {
                new KeyCensorElement("nested_dict_1_1_1", false),
                new KeyCensorElement("nested_dict_2_2", false),
                new KeyCensorElement("nested_array", false),
                new KeyCensorElement("null_key", false),
            };
            censors.CensorBodyElements(bodyCensors);
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

            const string postUrl = "http://httpbin.org/post";
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

        private static async Task<string?> GetJsonDataRequest(Cassette cassette, Mode mode)
        {
            var client = HttpClients.NewHttpClient(cassette, mode, new AdvancedSettings
            {
                MatchRules = MatchRules.DefaultStrict,
            });

            var fakeDataService = new FakeDataService(client);

            return await fakeDataService.GetJsonData();
        }
    }
}
