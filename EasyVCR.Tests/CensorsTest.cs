using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using EasyVCR.InternalUtilities.JSON;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyVCR.Tests
{
    [TestClass]
    public class CensorsTest
    {
        /// <summary>
        ///     Test that the default censor set is constructed without throwing an exception
        /// </summary>
        [TestMethod]
        public void TestDefaultCensors()
        {
            var censors = Censors.Default;

            // if we don't throw an exception by this point, we're good
            Assert.IsTrue(true);
        }

        /// <summary>
        ///     Test that, when we don't know the content type, we throw an exception
        /// </summary>
        [TestMethod]
        public void TestApplyBodyParametersCensorsNoContentType()
        {
            const string body = "{\"foo\":\"bar\"}";
            InternalUtilities.ContentType? contentType = null;

            var censors = new Censors();

            Assert.ThrowsException<VCRException>(() => censors.ApplyBodyParametersCensors(body, contentType));
        }

        /// <summary>
        ///     Test that, when the body is empty, we return the original body unmodified (empty string)
        /// </summary>
        [TestMethod]
        public void TestApplyBodyParametersCensorsEmptyStringReturnsOriginalString()
        {
            const string body = "";
            const InternalUtilities.ContentType contentType = InternalUtilities.ContentType.Json;

            var censors = new Censors();

            var result = censors.ApplyBodyParametersCensors(body, contentType);

            Assert.AreEqual(body, result);
        }

        /// <summary>
        ///     Test that, when no body censors are configured, we return the original body unmodified
        /// </summary>
        [TestMethod]
        public void TestApplyBodyParametersCensorsNoCensorsReturnsOriginalString()
        {
            const string body = "{\"foo\":\"bar\"}";
            const InternalUtilities.ContentType contentType = InternalUtilities.ContentType.Json;

            var censors = new Censors();

            var result = censors.ApplyBodyParametersCensors(body, contentType);

            Assert.AreEqual(body, result);
        }

        /// <summary>
        ///     Test that, when the content type is text (not JSON), we return the original body unmodified (we don't censor raw text currently)
        /// </summary>
        [TestMethod]
        public void TestApplyBodyParametersCensorsTextContentTypeReturnsOriginalString()
        {
            const string body = "text here";
            const InternalUtilities.ContentType contentType = InternalUtilities.ContentType.Text;

            var censors = new Censors();

            var result = censors.ApplyBodyParametersCensors(body, contentType);

            Assert.AreEqual(body, result);
        }

        /// <summary>
        ///     Test that, when the content type is HTML (not JSON), we return the original body unmodified (we don't censor HTML currently)
        /// </summary>
        [TestMethod]
        public void TestApplyBodyParametersCensorsHtmlContentTypeReturnsOriginalString()
        {
            const string body = "html code here";
            const InternalUtilities.ContentType contentType = InternalUtilities.ContentType.Html;

            var censors = new Censors();

            var result = censors.ApplyBodyParametersCensors(body, contentType);

            Assert.AreEqual(body, result);
        }

        /// <summary>
        ///     Test that, when the content type is XML (not JSON), we return the original body unmodified (we don't censor XML currently)
        /// </summary>
        [TestMethod]
        public void TestApplyBodyParametersCensorsXmlContentTypeReturnsOriginalString()
        {
            const string body = "xml code here";
            const InternalUtilities.ContentType contentType = InternalUtilities.ContentType.Xml;

            var censors = new Censors();

            var result = censors.ApplyBodyParametersCensors(body, contentType);

            Assert.AreEqual(body, result);
        }

        /// <summary>
        ///     Test that, when no headers are provided, we return the original headers unmodified (empty dictionary)
        /// </summary>
        [TestMethod]
        public void TestApplyHeaderCensorsNoHeadersReturnsOriginalHeaders()
        {
            var headers = new Dictionary<string, string> { };

            var censors = new Censors();

            var result = censors.ApplyHeaderCensors(headers);

            Assert.AreEqual(headers, result);
        }

        /// <summary>
        ///     Test that, when no header censors are configured, we return the original headers unmodified
        /// </summary>
        [TestMethod]
        public void TestApplyHeaderCensorsNoCensorsReturnsOriginalHeaders()
        {
            var headers = new Dictionary<string, string> { { "foo", "bar" } };

            var censors = new Censors();

            var result = censors.ApplyHeaderCensors(headers);

            Assert.AreEqual(headers, result);
        }

        /// <summary>
        ///     Test that, when no URL is provided, we return the original URL unmodified (null)
        /// </summary>
        [TestMethod]
        public void TestApplyUrlCensorsNoUrlReturnsOriginalUrl()
        {
            string? url = null;

            var censors = new Censors();

            var result = censors.ApplyUrlCensors(url);

            Assert.AreEqual(url, result);
        }

        /// <summary>
        ///     Test that, when no URL censors are configured, we return the original URL unmodified
        /// </summary>
        [TestMethod]
        public void TestApplyUrlCensorsNoQueryOrPathCensorsReturnsOriginalUrl()
        {
            const string url = "some url";

            var censors = new Censors();

            var result = censors.ApplyUrlCensors(url);

            Assert.AreEqual(url, result);
        }

        /// <summary>
        ///     Test that, when no path is provided, we return the original path unmodified (null)
        /// </summary>
        [TestMethod]
        public void TestApplyPathElementsCensorsNoUrlReturnsOriginalUrl()
        {
            string? url = null;

            var censors = new Censors();

            var result = censors.ApplyPathElementsCensors(url);

            Assert.AreEqual(url, result);
        }

        /// <summary>
        ///     Test that, when no path censors are configured, we return the original path unmodified
        /// </summary>
        [TestMethod]
        public void TestApplyPathElementsCensorsNoCensorsReturnsOriginalUrl()
        {
            const string url = "some url";

            var censors = new Censors();

            var result = censors.ApplyPathElementsCensors(url);

            Assert.AreEqual(url, result);
        }

        /// <summary>
        ///     Test TextCensorElement works for XML bodies
        /// </summary>
        [TestMethod]
        public async Task TestTextCensorOnXml()
        {
            var cassette = TestUtils.GetCassette("test_text_censor_on_xml");
            cassette.Erase(); // Erase cassette before recording

            // set up advanced settings
            var censorString = Guid.NewGuid().ToString(); // generate random string, high chance of not being in original data
            var advancedSettings = new AdvancedSettings
            {
                Censors = new Censors(censorString).CensorBodyElements(
                    new List<CensorElement>
                    {
                        // censor the word "my-label"
                        new TextCensorElement("my-label", false),
                    }),
            };

            // record cassette with advanced settings first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record, advancedSettings);
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetXmlDataRawResponse();

            // now replay cassette
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            fakeDataService = new FakeDataService(client);
            var xmlData = await fakeDataService.GetXmlData();

            Assert.IsNotNull(xmlData);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlData);

            // word "my-label" should be censored
            // for testing purposes, we know this exists as the "label" property of the "heading" node
            var nodes = xmlDocument.SelectNodes("//heading");
            Assert.IsNotNull(nodes);
            foreach (XmlNode node in nodes)
            {
                Assert.AreEqual(censorString, node.Attributes["label"].Value);
            }
        }

        /// <summary>
        ///     Test KeyCensorElement works for XML bodies
        /// </summary>
        [TestMethod]
        public async Task TestKeyCensorOnXml()
        {
            var cassette = TestUtils.GetCassette("test_key_censor_on_xml");
            cassette.Erase(); // Erase cassette before recording

            // set up advanced settings
            var censorString = Guid.NewGuid().ToString(); // generate random string, high chance of not being in original data
            var advancedSettings = new AdvancedSettings
            {
                Censors = new Censors(censorString).CensorBodyElements(
                    new List<CensorElement>
                    {
                        // censor the value of the "heading" key
                        new KeyCensorElement("heading", false),
                    }),
            };

            // record cassette with advanced settings first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record, advancedSettings);
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetXmlDataRawResponse();

            // now replay cassette
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            fakeDataService = new FakeDataService(client);
            var xmlData = await fakeDataService.GetXmlData();

            Assert.IsNotNull(xmlData);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlData);

            // whole value of "heading" node should be censored
            var nodes = xmlDocument.SelectNodes("//heading");
            Assert.IsNotNull(nodes);
            foreach (XmlNode node in nodes)
            {
                Assert.AreEqual(censorString, node.InnerText);
            }
        }

        /// <summary>
        ///     Test RegexCensorElement works for XML bodies
        /// </summary>
        [TestMethod]
        public async Task TestRegexCensorOnXml()
        {
            var cassette = TestUtils.GetCassette("test_regex_censor_on_xml");
            cassette.Erase(); // Erase cassette before recording

            // set up advanced settings
            var censorString = Guid.NewGuid().ToString(); // generate random string, high chance of not being in original data
            var advancedSettings = new AdvancedSettings
            {
                Censors = new Censors(censorString).CensorBodyElements(
                    new List<CensorElement>
                    {
                        // censor any value that looks like a date stamp
                        new RegexCensorElement(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}", false),
                    }),
            };

            // record cassette with advanced settings first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record, advancedSettings);
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetXmlDataRawResponse();

            // now replay cassette
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            fakeDataService = new FakeDataService(client);
            var xmlData = await fakeDataService.GetXmlData();

            Assert.IsNotNull(xmlData);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlData);

            // all values that look like date stamps should be censored
            // for testing purposes, we know this is stored in the "date" node
            var nodes = xmlDocument.SelectNodes("//date");
            Assert.IsNotNull(nodes);
            foreach (XmlNode node in nodes)
            {
                Assert.AreEqual(censorString, node.InnerText);
            }
        }

        [TestMethod]
        public async Task TestTextCensorOnHtml()
        {
            var cassette = TestUtils.GetCassette("test_text_censor_on_html");
            cassette.Erase(); // Erase cassette before recording

            // set up advanced settings
            var censorString = Guid.NewGuid().ToString(); // generate random string, high chance of not being in original data
            const string textToCensor = "This is some text";
            var advancedSettings = new AdvancedSettings
            {
                Censors = new Censors(censorString).CensorBodyElements(
                    new List<CensorElement>
                    {
                        // censor the text
                        new TextCensorElement(textToCensor, false),
                    }),
            };

            // record cassette with advanced settings first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record, advancedSettings);
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetHtmlDataRawResponse();

            // now replay cassette
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            fakeDataService = new FakeDataService(client);
            var textData = await fakeDataService.GetHtmlData();

            Assert.IsNotNull(textData);

            // censored word should no longer exist, and censor string should exist
            Assert.IsFalse(textData.Contains(textToCensor));
            Assert.IsTrue(textData.Contains(censorString));
        }

        [Ignore("Can't use KeyCensorElement on HTML bodies")]
        [TestMethod]
        public async Task TestKeyCensorOnHtml()
        {
            Assert.Fail("Can't use KeyCensorElement on HTML bodies");
        }

        [TestMethod]
        public async Task TestRegexCensorOnHtml()
        {
            var cassette = TestUtils.GetCassette("test_regex_censor_on_html");
            cassette.Erase(); // Erase cassette before recording

            // set up advanced settings
            var censorString = Guid.NewGuid().ToString(); // generate random string, high chance of not being in original data
            const string pattern = "</body>"; // censor the closing body tag
            var advancedSettings = new AdvancedSettings
            {
                Censors = new Censors(censorString).CensorBodyElements(
                    new List<CensorElement>
                    {
                        // censor the pattern
                        new RegexCensorElement(pattern, false),
                    }),
                MatchRules = new MatchRules().ByMethod().ByBaseUrl()
            };

            // record cassette with advanced settings first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record, advancedSettings);
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetHtmlDataRawResponse();

            // now replay cassette
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            fakeDataService = new FakeDataService(client);
            var textData = await fakeDataService.GetHtmlData();

            Assert.IsNotNull(textData);

            // censored pattern should no longer exist, and censor string should exist
            Assert.IsFalse(Regex.IsMatch(textData, pattern));
            Assert.IsTrue(textData.Contains(censorString));
        }

        /// <summary>
        ///     Test TextCensorElement works for plain text bodies
        /// </summary>
        [TestMethod]
        public async Task TestTextCensorOnText()
        {
            var cassette = TestUtils.GetCassette("test_text_censor_on_text");
            cassette.Erase(); // Erase cassette before recording

            // set up advanced settings
            var censorString = Guid.NewGuid().ToString(); // generate random string, high chance of not being in original data
            const string textToCensor = "free of charge, to any person obtaining";
            var advancedSettings = new AdvancedSettings
            {
                Censors = new Censors(censorString).CensorBodyElements(
                    new List<CensorElement>
                    {
                        // censor the text
                        new TextCensorElement(textToCensor, false),
                    }),
            };

            // record cassette with advanced settings first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record, advancedSettings);
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetRawDataRawResponse();

            // now replay cassette
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            fakeDataService = new FakeDataService(client);
            var textData = await fakeDataService.GetRawData();

            Assert.IsNotNull(textData);

            // censored word should no longer exist, and censor string should exist
            Assert.IsFalse(textData.Contains(textToCensor));
            Assert.IsTrue(textData.Contains(censorString));
        }

        [Ignore("Can't use KeyCensorElement on plain text bodies")]
        [TestMethod]
        public async Task TestKeyCensorOnText()
        {
            Assert.Fail("Can't use KeyCensorElement on plain text bodies");
        }

        /// <summary>
        ///     Test RegexCensorElement works for plain text bodies
        /// </summary>
        [TestMethod]
        public async Task TestRegexCensorOnText()
        {
            var cassette = TestUtils.GetCassette("test_regex_censor_on_text");
            cassette.Erase(); // Erase cassette before recording

            // set up advanced settings
            var censorString = Guid.NewGuid().ToString(); // generate random string, high chance of not being in original data
            const string pattern = "\\bCopyright \\(c\\)";
            var advancedSettings = new AdvancedSettings
            {
                Censors = new Censors(censorString).CensorBodyElements(
                    new List<CensorElement>
                    {
                        // censor the pattern
                        new RegexCensorElement(pattern, false),
                    }),
            };

            // record cassette with advanced settings first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record, advancedSettings);
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetRawDataRawResponse();

            // now replay cassette
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            fakeDataService = new FakeDataService(client);
            var textData = await fakeDataService.GetRawData();

            Assert.IsNotNull(textData);

            // censored pattern should no longer exist, and censor string should exist
            Assert.IsFalse(Regex.IsMatch(textData, pattern));
            Assert.IsTrue(textData.Contains(censorString));
        }

        /// <summary>
        ///     Test that we can mix and match censor elements
        /// </summary>
        [TestMethod]
        public async Task TestMixAndMatchCensorElements()
        {
            var cassette = TestUtils.GetCassette("test_mix_and_match_censor_elements");
            cassette.Erase(); // Erase cassette before recording

            // set up advanced settings
            var censorString = Guid.NewGuid().ToString(); // generate random string, high chance of not being in original data
            var advancedSettings = new AdvancedSettings
            {
                Censors = new Censors(censorString).CensorBodyElements(
                    new List<CensorElement>
                    {
                        // censor the word "label"
                        new TextCensorElement("label", false),
                        // censor the value of the "heading" key
                        new KeyCensorElement("heading", false), 
                        // censor any value that looks like a date stamp
                        new RegexCensorElement(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}", false),
                    }),
            };

            // record cassette with advanced settings first
            var client = HttpClients.NewHttpClient(cassette, Mode.Record, advancedSettings);
            var fakeDataService = new FakeDataService(client);
            var _ = await fakeDataService.GetXmlDataRawResponse();

            // now replay cassette
            client = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
            fakeDataService = new FakeDataService(client);
            var xmlData = await fakeDataService.GetXmlData();

            // check that the xml data was censored
            Assert.IsNotNull(xmlData);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlData);

            // word "my-label" should be censored
            // for testing purposes, we know this exists as the "label" property of the "heading" node
            var nodes = xmlDocument.SelectNodes("//heading");
            Assert.IsNotNull(nodes);
            foreach (XmlNode node in nodes)
            {
                Assert.AreEqual(censorString, node.Attributes["label"].Value);
            }

            // whole value of "heading" node should be censored
            nodes = xmlDocument.SelectNodes("//heading");
            Assert.IsNotNull(nodes);
            foreach (XmlNode node in nodes)
            {
                Assert.AreEqual(censorString, node.InnerText);
            }

            // all values that look like date stamps should be censored
            // for testing purposes, we know this is stored in the "date" node
            nodes = xmlDocument.SelectNodes("//date");
            Assert.IsNotNull(nodes);
            foreach (XmlNode node in nodes)
            {
                Assert.AreEqual(censorString, node.InnerText);
            }
        }

        [TestMethod]
        public async Task TestNonStringCensorKeyElements()
        {
            var cassette = TestUtils.GetCassette("test_non_string_censor_elements");
            cassette.Erase(); // Erase cassette before recording

            const string censorString = "censored-by-test";
            const int intToCensor = 123456;
            var dateToCensor = new DateTime(2020, 1, 1, 12, 0, 0);
            var booleanToCensor = true;
            var bodyObject = new
            {
                number = intToCensor,
                date = dateToCensor,
                boolean = booleanToCensor,
            };
            var body = Serialization.ConvertObjectToJson(bodyObject);
            const InternalUtilities.ContentType contentType = InternalUtilities.ContentType.Json;

            var bodyCensors = new List<KeyCensorElement>
            {
                new KeyCensorElement("number", false),
                new KeyCensorElement("date", false),
                new KeyCensorElement("boolean", false),
            };
            var censors = new Censors(censorString).CensorBodyElements(bodyCensors);

            var result = censors.ApplyBodyParametersCensors(body, contentType);

            Assert.AreEqual("{\"number\":\"censored-by-test\",\"date\":\"censored-by-test\",\"boolean\":\"censored-by-test\"}", result);
        }

        [TestMethod]
        public async Task TestNonStringCensorTextElements()
        {
            var cassette = TestUtils.GetCassette("test_non_string_censor_text_elements");
            cassette.Erase(); // Erase cassette before recording

            const string censorString = "censored-by-test";
            const int intToCensor = 123456;
            var dateToCensor = new DateTime(2020, 1, 1, 12, 0, 0);
            var booleanToCensor = true;
            var bodyObject = new
            {
                number = intToCensor,
                date = dateToCensor,
                boolean = booleanToCensor,
            };
            var body = Serialization.ConvertObjectToJson(bodyObject);
            const InternalUtilities.ContentType contentType = InternalUtilities.ContentType.Json;

            var bodyCensors = new List<TextCensorElement>
            {
                new TextCensorElement(intToCensor, false),
                new TextCensorElement(dateToCensor, false),
                new TextCensorElement(booleanToCensor, false),
            };
            var censors = new Censors(censorString).CensorBodyElements(bodyCensors);

            var result = censors.ApplyBodyParametersCensors(body, contentType);

            Assert.AreEqual(
                "{\"number\":\"censored-by-test\",\"date\":\"censored-by-test\",\"boolean\":\"censored-by-test\"}",
                result);
        }

        [TestMethod]
        public async Task TestNonStringCensorRegexElements()
        {
            var cassette = TestUtils.GetCassette("test_non_string_censor_regex_elements");
            cassette.Erase(); // Erase cassette before recording

            const string censorString = "censored-by-test";
            const int intToCensor = 123456;
            var booleanToCensor = true;
            var bodyObject = new
            {
                number = intToCensor,
                boolean = booleanToCensor,
            };
            var body = Serialization.ConvertObjectToJson(bodyObject);
            const InternalUtilities.ContentType contentType = InternalUtilities.ContentType.Json;

            var bodyCensors = new List<RegexCensorElement>
            {
                new RegexCensorElement(@"\b123456\b", false),
                // Date-to-string serialization is inconsistent, so excluding from test
                new RegexCensorElement(@"\btrue\b", false),
            };
            var censors = new Censors(censorString).CensorBodyElements(bodyCensors);

            var result = censors.ApplyBodyParametersCensors(body, contentType);

            Assert.AreEqual(
                "{\"number\":\"censored-by-test\",\"boolean\":\"censored-by-test\"}",
                result);
        }
    }
}
