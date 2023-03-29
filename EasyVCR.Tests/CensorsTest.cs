using System.Collections.Generic;
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
        ///     Test that, when the body is empty, we return the original body unmodified
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
        ///     Test that, when the content type is text (not JSON), we return the original body unmodified
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
        ///     Test that, when the content type is HTML (not JSON), we return the original body unmodified
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
        ///     Test that, when the content type is XML (not JSON), we return the original body unmodified
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
        ///     Test that, when no header are provided, we return the original headers unmodified (empty dictionary)
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
    }
}
