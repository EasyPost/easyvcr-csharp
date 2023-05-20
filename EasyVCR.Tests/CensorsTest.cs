using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
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
            var censorString = new Guid().ToString(); // generate random string, high chance of not being in original data
            var advancedSettings = new AdvancedSettings
            {
                Censors = new Censors(censorString).CensorBodyElements(
                    new List<CensorElement>
                    {
                        // censor the word "r/ProgrammerHumor"
                        new TextCensorElement("r/ProgrammerHumor", false),
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

            // word "r/ProgrammerHumor" should be censored
            // for testing purposes, we know this is the "label" property of the "category" node under "feed"
            var categoryNode = xmlDocument.FirstChild?.FirstChild;
            Assert.IsNotNull(categoryNode);
            Assert.AreEqual(censorString, categoryNode.Attributes["label"].Value);
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
            var censorString = new Guid().ToString(); // generate random string, high chance of not being in original data
            var advancedSettings = new AdvancedSettings
            {
                Censors = new Censors(censorString).CensorBodyElements(
                    new List<CensorElement>
                    {
                        // censor the value of the "title" key
                        new KeyCensorElement("title", false),
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

            // whole value of "title" key should be censored
            var nodes = xmlDocument.SelectNodes("//title");
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
            var censorString = new Guid().ToString(); // generate random string, high chance of not being in original data
            var advancedSettings = new AdvancedSettings
            {
                Censors = new Censors(censorString).CensorBodyElements(
                    new List<CensorElement>
                    {
                        // censor any value that looks like an date stamp
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

            // all values that look like urls should be censored
            // for testing purposes, we know this is stored in the "uri" nodes
            var nodes = xmlDocument.SelectNodes("//uri");
            Assert.IsNotNull(nodes);
            foreach (XmlNode node in nodes)
            {
                Assert.AreEqual(censorString, node.InnerText);
            }
        }

        [Ignore("Hard to test")]
        [TestMethod]
        public async Task TestTextCensorOnHtml()
        {
            // TextCensorHTML censors the whole text in the HTML body
            // Would need an HTML page with a small body to test this
            Assert.Fail();
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
            var censorString = new Guid().ToString(); // generate random string, high chance of not being in original data
            const string pattern = "<head>.*</head>";
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
            var censorString = new Guid().ToString(); // generate random string, high chance of not being in original data
            const string textToCensor = "# UGAArchive\nArchives of projects I did as a student at The University of Georgia\n";
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
            var censorString = new Guid().ToString(); // generate random string, high chance of not being in original data
            const string pattern = "^# UGAArchive";
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
            var censorString = new Guid().ToString(); // generate random string, high chance of not being in original data
            var advancedSettings = new AdvancedSettings
            {
                Censors = new Censors(censorString).CensorBodyElements(
                    new List<CensorElement>
                    {
                        // censor the word "r/ProgrammerHumor"
                        new TextCensorElement("r/ProgrammerHumor", false),
                        // censor the value of the "title" key
                        new KeyCensorElement("title", false), 
                        // censor any value that looks like an date stamp
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

            // word "r/ProgrammerHumor" should be censored
            // for testing purposes, we know this is the "label" property of the "category" node under "feed"
            var categoryNode = xmlDocument.FirstChild?.FirstChild;
            Assert.IsNotNull(categoryNode);
            Assert.AreEqual(censorString, categoryNode.Attributes["label"].Value);

            // whole value of "title" key should be censored
            var nodes = xmlDocument.SelectNodes("//title");
            Assert.IsNotNull(nodes);
            foreach (XmlNode node in nodes)
            {
                Assert.AreEqual(censorString, node.InnerText);
            }

            // all values that look like urls should be censored
            // for testing purposes, we know this is stored in the "uri" nodes
            nodes = xmlDocument.SelectNodes("//uri");
            Assert.IsNotNull(nodes);
            foreach (XmlNode node in nodes)
            {
                Assert.AreEqual(censorString, node.InnerText);
            }
        }
    }
}
