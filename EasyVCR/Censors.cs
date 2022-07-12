using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using EasyVCR.InternalUtilities;
using Newtonsoft.Json.Linq;
using JsonSerialization = EasyVCR.InternalUtilities.JSON.Serialization;
using XmlSerialization = EasyVCR.InternalUtilities.XML.Serialization;

// ReSharper disable UnusedMember.Global

namespace EasyVCR
{
    /// <summary>
    ///     Censoring capabilities for EasyVCR.
    /// </summary>
    public sealed class Censors
    {
        private readonly List<CensorElement> _bodyElementsToCensor;
        private readonly string _censorText = "*****";
        private readonly List<CensorElement> _headersToCensor;
        private readonly List<CensorElement> _queryParamsToCensor;

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
                censors.CensorHeadersByKeys(Defaults.CredentialHeadersToHide);
                censors.CensorQueryParametersByKeys(Defaults.CredentialParametersToHide);
                censors.CensorBodyElementsByKeys(Defaults.CredentialParametersToHide);

                return censors;
            }
        }

        /// <summary>
        ///     Initialize a new instance of the <see cref="Censors" /> factory.
        /// </summary>
        /// <param name="censorString">String to replace censored values with.</param>
        public Censors(string? censorString = null)
        {
            _queryParamsToCensor = new List<CensorElement>();
            _bodyElementsToCensor = new List<CensorElement>();
            _headersToCensor = new List<CensorElement>();
            _censorText = censorString ?? _censorText;
        }

        /// <summary>
        ///     Add a rule to censor specified body elements.
        /// </summary>
        /// <param name="elements">List of body elements to censor.</param>
        /// <returns>The current Censor object.</returns>
        public Censors CensorBodyElements(IEnumerable<CensorElement> elements)
        {
            _bodyElementsToCensor.AddRange(elements);
            return this;
        }

        /// <summary>
        ///     Add a rule to censor specified body elements by their keys.
        /// </summary>
        /// <param name="elementKeys">List of keys of body elements to censor.</param>
        /// <param name="caseSensitive">Whether to match case sensitively.</param>
        /// <returns></returns>
        public Censors CensorBodyElementsByKeys(List<string> elementKeys, bool caseSensitive = false)
        {
            foreach (var key in elementKeys)
            {
                _bodyElementsToCensor.Add(new CensorElement(key, caseSensitive));
            }

            return this;
        }

        /// <summary>
        ///     Add a rule to censor specified headers.
        ///     Note: This will censor the header keys in both the request and response.
        /// </summary>
        /// <param name="headers">List of headers to censor.</param>
        /// <returns>The current Censor object.</returns>
        public Censors CensorHeaders(IEnumerable<CensorElement> headers)
        {
            _headersToCensor.AddRange(headers);
            return this;
        }

        /// <summary>
        ///     Add a rule to censor specified headers by their keys.
        ///     Note: This will censor the header keys in both the request and response.
        /// </summary>
        /// <param name="headerKeys">List of keys of header to censor.</param>
        /// <param name="caseSensitive">Whether to match case sensitively.</param>
        /// <returns>The current Censor object.</returns>
        public Censors CensorHeadersByKeys(List<string> headerKeys, bool caseSensitive = false)
        {
            foreach (var key in headerKeys)
            {
                _headersToCensor.Add(new CensorElement(key, caseSensitive));
            }

            return this;
        }

        /// <summary>
        ///     Add a rule to censor specified query parameters.
        /// </summary>
        /// <param name="elements">List of query parameters to censor.</param>
        /// <returns>The current Censor object.</returns>
        public Censors CensorQueryParameters(IEnumerable<CensorElement> elements)
        {
            _queryParamsToCensor.AddRange(elements);
            return this;
        }

        /// <summary>
        ///     Add a rule to censor specified query parameters by their keys.
        /// </summary>
        /// <param name="parameterKeys">List of keys of query parameters to censor.</param>
        /// <param name="caseSensitive">Whether to match case sensitively.</param>
        /// <returns>The current Censor object.</returns>
        public Censors CensorQueryParametersByKeys(List<string> parameterKeys, bool caseSensitive = false)
        {
            foreach (var key in parameterKeys)
            {
                _queryParamsToCensor.Add(new CensorElement(key, caseSensitive));
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
        internal string ApplyBodyParametersCensors(string body, ContentType? contentType)
        {
            if (contentType == null) throw new VCRException("Cannot determine content type of response body, unable to apply censors.");

            if (string.IsNullOrWhiteSpace(body))
                // short circuit if body is null or empty
                return body;

            if (_bodyElementsToCensor.Count == 0)
            {
                // short circuit if there are no censors to apply
                return body;
            }

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
                        return CensorJsonData(body, _censorText, _bodyElementsToCensor);
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
        internal IDictionary<string, string> ApplyHeaderCensors(IDictionary<string, string> headers)
        {
            if (headers.Count == 0)
                // short circuit if there are no headers to censor
                return headers;

            return _headersToCensor.Count == 0 ? headers : headers.ToDictionary(header => header.Key, header => ElementShouldBeCensored(header.Key, _headersToCensor) ? _censorText : header.Value);
        }

        /// <summary>
        ///     Censor the appropriate query parameters.
        /// </summary>
        /// <param name="url">Full URL string to apply censors to.</param>
        /// <returns>Censored URL string.</returns>
        internal string? ApplyQueryParametersCensors(string? url)
        {
            if (_queryParamsToCensor.Count == 0)
            {
                // short circuit if there are no censors to apply
                return url;
            }

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
                if (key == null)
                    // short circuit if key is null
                    continue;
                censoredQueryParameters.Add(key, ElementShouldBeCensored(key, _queryParamsToCensor) ? _censorText : queryParameters[key]);
            }

            return $"{uri.GetLeftPart(UriPartial.Path)}?{ToQueryString(censoredQueryParameters)}";
        }

        /// <summary>
        ///     Apply censors to a JSON string.
        /// </summary>
        /// <param name="data">JSON string to apply censors to.</param>
        /// <param name="censorText">Test to use to replace censored elements.</param>
        /// <param name="elementsToCensors">List of elements to censor.</param>
        /// <returns>A censored JSON string.</returns>
        public static string CensorJsonData(string data, string censorText, IReadOnlyCollection<CensorElement> elementsToCensors)
        {
            try
            {
                var jsonDictionary = JsonSerialization.ConvertJsonToObject<Dictionary<string, object>>(data);
                var censoredJsonDictionary = ApplyDataCensors(jsonDictionary, censorText, elementsToCensors);
                return JsonSerialization.ConvertObjectToJson(censoredJsonDictionary);
            }
            catch (Exception)
            {
                // body is not a JSON dictionary
                try
                {
                    var jsonList = JsonSerialization.ConvertJsonToObject<List<object>>(data);
                    var censoredJsonList = ApplyDataCensors(jsonList, censorText, elementsToCensors);
                    return JsonSerialization.ConvertObjectToJson(censoredJsonList);
                }
                catch
                {
                    // short circuit if body is not a JSON dictionary or JSON list
                    return data;
                }
            }
        }

        /// <summary>
        ///     Apply censors to an XML string.
        /// </summary>
        /// <param name="data">XML string to apply censors to.</param>
        /// <param name="censorText">Test to use to replace censored elements.</param>
        /// <param name="elementsToCensors">List of elements to censor.</param>
        /// <returns>A censored XML string.</returns>
        public static string CensorXmlData(string data, string censorText, IReadOnlyCollection<CensorElement> elementsToCensors)
        {
            try
            {
                var xmlDictionary = XmlSerialization.ConvertXmlToObject<Dictionary<string, object>>(data);
                var censoredXmlDictionary = ApplyDataCensors(xmlDictionary, censorText, elementsToCensors);
                return XmlSerialization.ConvertObjectToXml(censoredXmlDictionary);
            }
            catch (Exception)
            {
                // body is not an XML dictionary
                try
                {
                    var xmlList = XmlSerialization.ConvertXmlToObject<List<object>>(data);
                    var censoredXmlList = ApplyDataCensors(xmlList, censorText, elementsToCensors);
                    return XmlSerialization.ConvertObjectToXml(censoredXmlList);
                }
                catch
                {
                    // short circuit if body is not a XML dictionary or XML list
                    return data;
                }
            }
        }

        /// <summary>
        ///     Apply censors to a list of elements.
        /// </summary>
        /// <param name="list">List of elements to apply censors to.</param>
        /// <param name="censorText">Test to use to replace censored elements.</param>
        /// <param name="elementsToCensors">List of elements to censor.</param>
        /// <returns>A censored list of elements.</returns>
        private static List<object> ApplyDataCensors(List<object> list, string censorText, IReadOnlyCollection<CensorElement> elementsToCensors)
        {
            if (list.Count == 0)
                // short circuit if there are no body parameters
                return list;

            var censoredList = new List<object>();
            foreach (var entry in list)
            {
                if (Utilities.IsJsonDictionary(entry))
                {
                    var entryDict = ((JObject)entry).ToObject<Dictionary<string, object>>();
                    if (entryDict == null)
                    {
                        // could not convert to dictionary, so skip (this should never happen)
                        censoredList.Add(entry);
                    }
                    else
                    {
                        var censoredEntryDict = ApplyDataCensors(entryDict, censorText, elementsToCensors);
                        censoredList.Add(censoredEntryDict);
                    }
                }
                else if (Utilities.IsJsonArray(entry))
                {
                    var entryList = ((JArray)entry).ToObject<List<object>>();
                    if (entryList == null)
                    {
                        // could not convert to list, so skip (this should never happen)
                        censoredList.Add(entry);
                    }
                    else
                    {
                        var censoredEntryList = ApplyDataCensors(entryList, censorText, elementsToCensors);
                        censoredList.Add(censoredEntryList);
                    }
                }
                else
                {
                    // either a primitive or null, no censoring needed
                    censoredList.Add(entry);
                }
            }

            return censoredList;
        }

        /// <summary>
        ///     Apply censors to a dictionary of elements.
        /// </summary>
        /// <param name="dictionary">Dictionary of elements to apply censors to.</param>
        /// <param name="censorText">Test to use to replace censored elements.</param>
        /// <param name="elementsToCensors">List of elements to censor.</param>
        /// <returns>A censored dictionary of elements.</returns>
        private static Dictionary<string, object> ApplyDataCensors(Dictionary<string, object> dictionary, string censorText, IReadOnlyCollection<CensorElement> elementsToCensors)
        {
            if (dictionary.Count == 0)
                // short circuit if there are no body parameters
                return dictionary;

            var censoredBodyDictionary = new Dictionary<string, object>();
            foreach (var key in dictionary.Keys)
            {
                if (ElementShouldBeCensored(key, elementsToCensors))
                {
                    var value = dictionary[key];
                    if (value == null)
                    {
                        // don't need to worry about censoring something that's null (don't replace null with the censor string)
                        continue;
                    }

                    if (Utilities.IsJsonDictionary(value))
                    {
                        // replace with empty dictionary
                        censoredBodyDictionary.Add(key, new Dictionary<string, object>());
                    }
                    else if (Utilities.IsJsonArray(value))
                    {
                        // replace with empty array
                        censoredBodyDictionary.Add(key, new List<object>());
                    }
                    else
                    {
                        // replace with censor text
                        censoredBodyDictionary.Add(key, censorText);
                    }
                }
                else
                {
                    var value = dictionary[key];

                    if (Utilities.IsJsonDictionary(value))
                    {
                        // recursively censor inner dictionaries
                        var valueDict = ((JObject)dictionary[key]).ToObject<Dictionary<string, object>>();
                        if (valueDict != null)
                        {
                            // change the value if can be parsed as a dictionary (otherwise, skip censoring)
                            value = ApplyDataCensors(valueDict, censorText, elementsToCensors);
                        }
                    }

                    else if (Utilities.IsJsonArray(value))
                    {
                        // recursively censor list elements
                        var valueList = ((JArray)dictionary[key]).ToObject<List<object>>();
                        if (valueList != null)
                        {
                            value = ApplyDataCensors(valueList, censorText, elementsToCensors);
                        }
                    }

                    censoredBodyDictionary.Add(key, value);
                }
            }

            return censoredBodyDictionary;
        }

        /// <summary>
        ///     Check if a JSON element should be censored.
        /// </summary>
        /// <param name="foundKey">The key of the JSON element to evaluate.</param>
        /// <param name="elementsToCensor">A list of elements to censor.</param>
        /// <returns>True if the JSON value should be censored, false otherwise.</returns>
        private static bool ElementShouldBeCensored(string foundKey, IReadOnlyCollection<CensorElement> elementsToCensor)
        {
            return elementsToCensor.Count != 0 && elementsToCensor.Any(element => element.Matches(foundKey));
        }

        /// <summary>
        ///     Convert a collection of query parameter pairs to a query string.
        /// </summary>
        /// <param name="queryParamCollection">Collection of key-value pairs.</param>
        /// <returns>A formatted URL query string.</returns>
        private static string ToQueryString(NameValueCollection queryParamCollection)
        {
            return string.Join("&", queryParamCollection.AllKeys.Select(key => $"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(queryParamCollection.Get(key))}").ToArray());
        }
    }
}
