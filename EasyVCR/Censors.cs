using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using EasyVCR.InternalUtilities;
using Newtonsoft.Json.Linq;
using JsonSerialization = EasyVCR.InternalUtilities.JSON.Serialization;
using XmlSerialization = EasyVCR.InternalUtilities.XML.Serialization;

namespace EasyVCR
{
    /// <summary>
    ///     Censoring capabilities for EasyVCR.
    /// </summary>
    public sealed class Censors
    {
        private readonly List<string> _bodyParamsToCensor;
        private readonly bool _caseSensitive;
        private readonly string _censorText = "*****";
        private readonly List<string> _headersToCensor;
        private readonly List<string> _queryParamsToCensor;

        /// <summary>
        ///     Default censors is to not censor anything.
        /// </summary>
        public static Censors Default => new Censors();

        /// <summary>
        ///     Default sensitive censors is to censor common private information (i.e. API keys, auth tokens, etc.)
        /// </summary>
        public static Censors DefaultSensitive
        {
            get
            {
                var censors = new Censors();
                foreach (var key in Defaults.CredentialHeadersToHide) censors.HideHeader(key);

                foreach (var key in Defaults.CredentialParametersToHide)
                {
                    censors.HideQueryParameter(key);
                    censors.HideBodyParameter(key);
                }

                return censors;
            }
        }

        /// <summary>
        ///     Initialize a new instance of the <see cref="Censors" /> factory.
        /// </summary>
        /// <param name="censorString">String to replace censored values with.</param>
        /// <param name="caseSensitive">Whether to enforce case when finding keys to censor</param>
        public Censors(string? censorString = null, bool caseSensitive = false)
        {
            _queryParamsToCensor = new List<string>();
            _bodyParamsToCensor = new List<string>();
            _headersToCensor = new List<string>();
            _censorText = censorString ?? _censorText;
            _caseSensitive = caseSensitive;
        }

        /// <summary>
        ///     Add a rule to censor a specified body parameter.
        ///     Note: Only top-level pairs can be censored.
        /// </summary>
        /// <param name="parameterKey">Key of body parameter to censor.</param>
        /// <returns></returns>
        public Censors HideBodyParameter(string parameterKey)
        {
            _bodyParamsToCensor.Add(_caseSensitive ? parameterKey : parameterKey.ToLowerInvariant());
            return this;
        }

        /// <summary>
        ///     Add a rule to censor specified body parameters.
        ///     Note: Only top-level pairs can be censored.
        /// </summary>
        /// <param name="parameterKeys">List of keys of body parameter to censor.</param>
        /// <returns></returns>
        public Censors HideBodyParameters(List<string> parameterKeys)
        {
            foreach (var key in parameterKeys)
            {
                HideBodyParameter(key);
            }

            return this;
        }

        /// <summary>
        ///     Add a rule to censor a specified header key.
        ///     Note: This will censor the header key in both the request and response.
        /// </summary>
        /// <param name="headerKey">Key of header to censor.</param>
        /// <returns>The current Censor object.</returns>
        public Censors HideHeader(string headerKey)
        {
            _headersToCensor.Add(_caseSensitive ? headerKey : headerKey.ToLowerInvariant());
            return this;
        }

        /// <summary>
        ///     Add a rule to censor specified header keys.
        ///     Note: This will censor the header keys in both the request and response.
        /// </summary>
        /// <param name="headerKeys">List of keys of header to censor.</param>
        /// <returns>The current Censor object.</returns>
        public Censors HideHeaders(List<string> headerKeys)
        {
            foreach (var key in headerKeys)
            {
                HideHeader(key);
            }

            return this;
        }

        /// <summary>
        ///     Add a rule to censor a specified query parameter.
        /// </summary>
        /// <param name="parameterKey">Key of query parameter to censor.</param>
        /// <returns>The current Censor object.</returns>
        public Censors HideQueryParameter(string parameterKey)
        {
            _queryParamsToCensor.Add(_caseSensitive ? parameterKey : parameterKey.ToLowerInvariant());
            return this;
        }

        /// <summary>
        ///     Add a rule to censor specified query parameters.
        /// </summary>
        /// <param name="parameterKeys">List of keys of query parameter to censor.</param>
        /// <returns>The current Censor object.</returns>
        public Censors HideQueryParameters(List<string> parameterKeys)
        {
            foreach (var key in parameterKeys)
            {
                HideQueryParameter(key);
            }

            return this;
        }

        /// <summary>
        ///     Censor the appropriate body parameters.
        /// </summary>
        /// <param name="body">String representation of request body to apply censors to.</param>
        /// <param name="contentType">ContentType enum indicating what type of content body is.</param>
        /// <returns>Censored string representation of request body.</returns>
        /// <exception cref="SerializeException">Could not serialize data to apply censors.</exception>
        internal string CensorBodyParameters(string body, ContentType? contentType)
        {
            if (contentType == null) throw new VCRException("Cannot determine content type of response body, unable to apply censors.");

            if (string.IsNullOrWhiteSpace(body))
                // short circuit if body is null or empty
                return body;

            try
            {
                switch (contentType)
                {
                    case ContentType.Text:
                    case ContentType.Html:
                        return body; // We can't censor plaintext bodies or HTML bodies.
                    case ContentType.Xml:
                        return body; // XML parsing is not supported yet, so we can't censor XML bodies.
                    case ContentType.Json:
                        return CensorJsonBodyParameters(body);
                    default:
                        throw new VCRException("Unrecognized content type: " + contentType);
                }
            }
            catch (SerializeException)
            {
                // short circuit if body is not a valid serializable type
                throw new VCRException("Body is not valid serializable type");
            }
        }

        /// <summary>
        ///     Censor the appropriate headers.
        /// </summary>
        /// <param name="headers">Dictionary of headers to apply censors to.</param>
        /// <returns>Censored IDictionary object.</returns>
        internal IDictionary<string, string> CensorHeaders(IDictionary<string, string> headers)
        {
            if (headers.Count == 0)
                // short circuit if there are no headers to censor
                return headers;

            var censoredHeaders = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                censoredHeaders.Add(header.Key, KeyShouldBeCensored(header.Key, _headersToCensor) ? _censorText : header.Value);
            }

            return censoredHeaders;
        }

        /// <summary>
        ///     Censor the appropriate query parameters.
        /// </summary>
        /// <param name="url">Full URL string to apply censors to.</param>
        /// <returns>Censored URL string.</returns>
        internal string? CensorQueryParameters(string? url)
        {
            if (url == null)
                // short circuit if url is null
                return url;
            var uri = new Uri(url);
            var queryParameters = HttpUtility.ParseQueryString(uri.Query);

            if (queryParameters.Count == 0)
                // short circuit if there are no query parameters
                return url;

            var censoredQueryParameters = new NameValueCollection();
            foreach (var key in queryParameters.AllKeys)
            {
                censoredQueryParameters.Add(key, KeyShouldBeCensored(key, _queryParamsToCensor) ? _censorText : queryParameters[key]);
            }

            return $"{uri.GetLeftPart(UriPartial.Path)}?{ToQueryString(censoredQueryParameters)}";
        }

        private Dictionary<string, object>? ApplyBodyCensors(Dictionary<string, object> dictionary)
        {
            if (dictionary.Count == 0)
                // short circuit if there are no body parameters
                return null;

            var censoredBodyDictionary = new Dictionary<string, object>();
            foreach (var key in dictionary.Keys)
            {
                var value = dictionary[key];
                if (Utilities.IsJsonDictionary(value))
                {
                    var valueDict = ((JObject)dictionary[key]).ToObject<Dictionary<string, object>>();
                    if (valueDict != null)
                    {
                        value = ApplyBodyCensors(valueDict)!;
                    }
                }

                censoredBodyDictionary.Add(key, KeyShouldBeCensored(key, _bodyParamsToCensor) ? _censorText : value);
            }

            return censoredBodyDictionary;
        }

        private string CensorJsonBodyParameters(string body)
        {
            Dictionary<string, object> bodyDictionary;
            try
            {
                bodyDictionary = JsonSerialization.ConvertJsonToObject<Dictionary<string, object>>(body);
            }
            catch (Exception)
            {
                // short circuit if body is not a JSON dictionary
                return body;
            }

            var censoredBodyDictionary = ApplyBodyCensors(bodyDictionary);
            return censoredBodyDictionary == null ? body : JsonSerialization.ConvertObjectToJson(censoredBodyDictionary);
        }

        private string CensorXmlBodyParameters(string body)
        {
            Dictionary<string, object> bodyDictionary;
            try
            {
                bodyDictionary = XmlSerialization.ConvertXmlToObject<Dictionary<string, object>>(body);
            }
            catch (Exception)
            {
                // short circuit if body is not an XML dictionary
                return body;
            }

            var censoredBodyDictionary = ApplyBodyCensors(bodyDictionary);
            return censoredBodyDictionary == null ? body : XmlSerialization.ConvertObjectToXml(censoredBodyDictionary);
        }

        private bool KeyShouldBeCensored(string foundKey, List<string> keysToCensor)
        {
            // keysToCensor are already cased as needed
            if (!_caseSensitive)
            {
                foundKey = foundKey.ToLowerInvariant();
            }

            return keysToCensor.Contains(foundKey);
        }

        private static string ToQueryString(NameValueCollection collection)
        {
            return string.Join("&", collection.AllKeys.Select(key => $"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(collection.Get(key))}").ToArray());
        }
    }
}
