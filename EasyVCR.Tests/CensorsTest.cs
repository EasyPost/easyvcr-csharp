using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyVCR.Tests
{
    [TestClass]
    public class CensorsTest
    {
        [TestMethod]
        public void TestDefaultCensors()
        {
            var censors = Censors.Default;

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestApplyBodyParametersCensorsNoContentType()
        {
            const string body = "{\"foo\":\"bar\"}";
            InternalUtilities.ContentType? contentType = null;

            var censors = new Censors();

            Assert.ThrowsException<VCRException>(() => censors.ApplyBodyParametersCensors(body, contentType));
        }

        [TestMethod]
        public void TestApplyBodyParametersCensorsEmptyStringReturnsOriginalString()
        {
            const string body = "";
            const InternalUtilities.ContentType contentType = InternalUtilities.ContentType.Json;

            var censors = new Censors();

            var result = censors.ApplyBodyParametersCensors(body, contentType);

            Assert.AreEqual(body, result);
        }

        [TestMethod]
        public void TestApplyBodyParametersCensorsNoCensorsReturnsOriginalString()
        {
            const string body = "{\"foo\":\"bar\"}";
            const InternalUtilities.ContentType contentType = InternalUtilities.ContentType.Json;

            var censors = new Censors();

            var result = censors.ApplyBodyParametersCensors(body, contentType);

            Assert.AreEqual(body, result);
        }

        [TestMethod]
        public void TestApplyBodyParametersCensorsTextContentTypeReturnsOriginalString()
        {
            const string body = "text here";
            const InternalUtilities.ContentType contentType = InternalUtilities.ContentType.Text;

            var censors = new Censors();

            var result = censors.ApplyBodyParametersCensors(body, contentType);

            Assert.AreEqual(body, result);
        }

        [TestMethod]
        public void TestApplyBodyParametersCensorsHtmlContentTypeReturnsOriginalString()
        {
            const string body = "html code here";
            const InternalUtilities.ContentType contentType = InternalUtilities.ContentType.Html;

            var censors = new Censors();

            var result = censors.ApplyBodyParametersCensors(body, contentType);

            Assert.AreEqual(body, result);
        }

        [TestMethod]
        public void TestApplyBodyParametersCensorsXmlContentTypeReturnsOriginalString()
        {
            const string body = "xml code here";
            const InternalUtilities.ContentType contentType = InternalUtilities.ContentType.Xml;

            var censors = new Censors();

            var result = censors.ApplyBodyParametersCensors(body, contentType);

            Assert.AreEqual(body, result);
        }

        [TestMethod]
        public void TestApplyHeaderCensorsNoHeadersReturnsOriginalHeaders()
        {
            var headers = new Dictionary<string, string> { };

            var censors = new Censors();

            var result = censors.ApplyHeaderCensors(headers);

            Assert.AreEqual(headers, result);
        }

        [TestMethod]
        public void TestApplyHeaderCensorsNoCensorsReturnsOriginalHeaders()
        {
            var headers = new Dictionary<string, string> { { "foo", "bar" } };

            var censors = new Censors();

            var result = censors.ApplyHeaderCensors(headers);

            Assert.AreEqual(headers, result);
        }

        [TestMethod]
        public void TestApplyUrlCensorsNoUrlReturnsOriginalUrl()
        {
            string? url = null;

            var censors = new Censors();

            var result = censors.ApplyUrlCensors(url);

            Assert.AreEqual(url, result);
        }

        [TestMethod]
        public void TestApplyUrlCensorsNoQueryOrPathCensorsReturnsOriginalUrl()
        {
            const string url = "some url";

            var censors = new Censors();

            var result = censors.ApplyUrlCensors(url);

            Assert.AreEqual(url, result);
        }

        [TestMethod]
        public void TestApplyPathElementsCensorsNoUrlReturnsOriginalUrl()
        {
            string? url = null;

            var censors = new Censors();

            var result = censors.ApplyPathElementsCensors(url);

            Assert.AreEqual(url, result);
        }

        [TestMethod]
        public void TestApplyPathElementsCensorsNoCensorsReturnsOriginalUrl()
        {
            const string url = "some url";

            var censors = new Censors();

            var result = censors.ApplyPathElementsCensors(url);

            Assert.AreEqual(url, result);
        }
    }
}
