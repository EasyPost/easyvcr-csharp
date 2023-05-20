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
        private readonly List<RegexCensorElement> _pathElementsToCensor;

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

                var headerCensors = Defaults.CredentialHeadersToHide.Select(header => new KeyCensorElement(header, false)).ToList();
                censors.CensorHeaders(headerCensors);

                var queryParamCensors = Defaults.CredentialParametersToHide.Select(queryParam => new KeyCensorElement(queryParam, false)).ToList();
                censors.CensorQueryParameters(queryParamCensors);

                var bodyElementCensors = Defaults.CredentialParametersToHide.Select(bodyElement => new KeyCensorElement(bodyElement, false)).ToList();
                censors.CensorBodyElements(bodyElementCensors);

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
            _pathElementsToCensor = new List<RegexCensorElement>();
            _censorText = censorString ?? _censorText;
        }

        /// <summary>
        ///     Add a rule to censor specified body elements.
        /// </summary>
        /// <param name="elements">
        ///     List of body elements to censor.<br/>
        ///     JSON: Use <see cref="KeyCensorElement"/> or <see cref="RegexCensorElement"/><br/>. Any <see cref="TextCensorElement"/> will be ignored.<br/>
        ///     XML: Use <see cref="KeyCensorElement"/> or <see cref="RegexCensorElement"/><br/>. Any <see cref="TextCensorElement"/> will be ignored.<br/>
        ///     HTML: Use <see cref="TextCensorElement"/> or <see cref="RegexCensorElement"/><br/>. Any <see cref="KeyCensorElement"/> will be ignored.<br/>
        ///     Plain text: Use <see cref="TextCensorElement"/> or <see cref="RegexCensorElement"/>. Any <see cref="KeyCensorElement"/> will be ignored.<br/>
        /// </param>
        /// <returns>The current Censor object.</returns>
        public Censors CensorBodyElements(IEnumerable<CensorElement> elements)
        {
            _bodyElementsToCensor.AddRange(elements);
            return this;
        }

        /// <summary>
        ///     Add a rule to censor specified headers.
        ///     Note: This will censor the header keys in both the request and response.
        /// </summary>
        /// <param name="headers">List of headers to censor.</param>
        /// <returns>The current Censor object.</returns>
        public Censors CensorHeaders(IEnumerable<KeyCensorElement> headers)
        {
            _headersToCensor.AddRange(headers);
            return this;
        }

        /// <summary>
        ///     Add a rule to censor specified query parameters.
        /// </summary>
        /// <param name="elements">List of query parameters to censor.</param>
        /// <returns>The current Censor object.</returns>
        public Censors CensorQueryParameters(IEnumerable<KeyCensorElement> elements)
        {
            _queryParamsToCensor.AddRange(elements);
            return this;
        }

        /// <summary>
        ///     Add a rule to censor specified path elements.
        /// </summary>
        /// <param name="elements">List of path elements to censor.</param>
        /// <returns>The current Censor object.</returns>
        public Censors CensorPathElements(IEnumerable<RegexCensorElement> elements)
        {
            _pathElementsToCensor.AddRange(elements);
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
            {
                // short circuit if body is null or empty
                return body;
            }

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
                        return CensorTextData(body, _censorText, _bodyElementsToCensor);
                    case ContentType.Xml:
                        return CensorXmlData(body, _censorText, _bodyElementsToCensor);
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
            {
                // short circuit if there are no headers to censor
                return headers;
            }

            return _headersToCensor.Count == 0 ? headers : headers.ToDictionary(header => header.Key, header => ElementShouldBeCensored(foundValue: header.Value, foundKey: header.Key, _headersToCensor) ? _censorText : header.Value);
        }

        /// <summary>
        ///     Censor the appropriate path elements and query parameters.
        /// </summary>
        /// <param name="url">Full URL string to apply censors to.</param>
        /// <returns>Censored URL string.</returns>
        internal string? ApplyUrlCensors(string? url)
        {
            if (url == null)
            {
                // short circuit if url is null
                return url;
            }

            if (_queryParamsToCensor.Count == 0 && _pathElementsToCensor.Count == 0)
            {
                // short circuit if there are no censors to apply
                return url;
            }

            var uri = new Uri(url);

            var path = uri.GetLeftPart(UriPartial.Path); // bad function name, Microsoft. This gets the indicated portion of a URI (here, the full path minus query), not the left part of the path.
            if (path.EndsWith("?"))
            {
                path = path.Substring(0, path.Length - 1);
            }
            var query = uri.Query;
            var queryParameters = HttpUtility.ParseQueryString(query);

            string censoredPath;
            string? censoredQueryString;

            if (_pathElementsToCensor.Count == 0)
            {
                // don't need to censor path elements
                censoredPath = path;
            }
            else
            {
                // censor path elements
                var tempPath = path;
                foreach (var pathCensor in _pathElementsToCensor)
                {
                    tempPath = pathCensor.MatchAndReplaceAsNeeded(tempPath, _censorText);
                }

                censoredPath = tempPath;
            }

            if (queryParameters.Count == 0)
            {
                // no query parameters to censor
                censoredQueryString = null;
            }
            else
            {
                if (_queryParamsToCensor.Count == 0)
                {
                    // don't need to censor query parameters
                    censoredQueryString = query;
                }
                else
                {
                    // censor query parameters
                    var censoredQueryParameters = new NameValueCollection();
                    // iterate through each element in NameValueCollection
                    foreach (var key in queryParameters.AllKeys)
                    {
                        if (key == null)
                        {
                            // short circuit if key is null
                            continue;
                        }

                        var foundValue = queryParameters[key];
                        censoredQueryParameters.Add(key, ElementShouldBeCensored(foundValue: foundValue, foundKey: key, _queryParamsToCensor) ? _censorText : queryParameters[key]);
                    }

                    censoredQueryString = ToQueryString(censoredQueryParameters);
                }
            }

            // build censored url
            var censoredUrl = censoredPath;
            if (censoredQueryString != null)
            {
                censoredUrl += $"?{censoredQueryString}";
            }

            return censoredUrl;
        }

        /// <summary>
        ///     Censor the appropriate path elements.
        /// </summary>
        /// <param name="url">Full URL string to apply censors to.</param>
        /// <returns>URL string with path elements censored. Query parameters will not be censored as part of this process.</returns>
        internal string? ApplyPathElementsCensors(string? url)
        {
            if (url == null)
            {
                // short circuit if url is null
                return url;
            }

            if (_pathElementsToCensor.Count == 0)
            {
                // short circuit if there are no censors to apply
                return url;
            }

            var uri = new Uri(url);
            var queryParameters = HttpUtility.ParseQueryString(uri.Query);

            var path = uri.GetLeftPart(UriPartial.Path); // bad function name, Microsoft. This gets the indicated portion of a URI (here, the full path minus query), not the left part of the path.

            foreach (var pathCensor in _pathElementsToCensor)
            {
                path = pathCensor.MatchAndReplaceAsNeeded(path, _censorText);
            }

            var censoredUrl = path;

            if (queryParameters.Count > 0)
            {
                censoredUrl = $"{censoredUrl}?{ToQueryString(queryParameters)}";
            }

            return censoredUrl;
        }

        /// <summary>
        ///     Apply censors to a raw text string.
        /// </summary>
        /// <param name="data">Raw text string to apply censors to.</param>
        /// <param name="censorText">Text to use to replace censored elements.</param>
        /// <param name="elementsToCensor">List of elements to censor.</param>
        /// <returns>A censored raw text string.</returns>
        public static string CensorTextData(string data, string censorText, IReadOnlyCollection<CensorElement> elementsToCensor)
        {
            if (elementsToCensor.Count == 0)
            {
                // short circuit if there are no censors to apply
                return data;
            }

            var censoredData = data;
            foreach (var censorElement in elementsToCensor)
            {
                switch (censorElement)
                {
                    // we cannot process KeyCensorElements in raw text/html
                    case KeyCensorElement _:
                        continue;
                    case RegexCensorElement regexCensorElement:
                        censoredData = regexCensorElement.MatchAndReplaceAsNeeded(censoredData, censorText);
                        break;
                    case TextCensorElement textCensorElement:
                        censoredData = textCensorElement.MatchAndReplaceAsNeeded(censoredData, censorText);
                        break;
                }
            }

            return censoredData;
        }

        /// <summary>
        ///     Apply censors to a JSON string.
        /// </summary>
        /// <param name="data">JSON string to apply censors to.</param>
        /// <param name="censorText">Text to use to replace censored elements.</param>
        /// <param name="elementsToCensors">List of elements to censor.</param>
        /// <returns>A censored JSON string.</returns>
        public static string CensorJsonData(string data, string censorText, IReadOnlyCollection<CensorElement> elementsToCensors)
        {
            try
            {
                var jsonDictionary = JsonSerialization.ConvertJsonToObject<Dictionary<string, object>>(data);
                var censoredJsonDictionary = ApplyJsonXmlDataCensors(jsonDictionary, censorText, elementsToCensors);
                return JsonSerialization.ConvertObjectToJson(censoredJsonDictionary);
            }
            catch (Exception)
            {
                // body is not a JSON dictionary
                try
                {
                    var jsonList = JsonSerialization.ConvertJsonToObject<List<object>>(data);
                    var censoredJsonList = ApplyJsonXmlDataCensors(jsonList, censorText, elementsToCensors);
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
        /// <param name="censorText">Text to use to replace censored elements.</param>
        /// <param name="elementsToCensors">List of elements to censor.</param>
        /// <returns>A censored XML string.</returns>
        public static string CensorXmlData(string data, string censorText, IReadOnlyCollection<CensorElement> elementsToCensors)
        {
            try
            {
                var xmlDictionary = XmlSerialization.ConvertXmlToObject<Dictionary<string, object>>(data);
                var censoredXmlDictionary = ApplyJsonXmlDataCensors(xmlDictionary, censorText, elementsToCensors);
                return XmlSerialization.ConvertObjectToXml(censoredXmlDictionary);
            }
            catch (Exception)
            {
                // body is not an XML dictionary
                try
                {
                    var xmlList = XmlSerialization.ConvertXmlToObject<List<object>>(data);
                    var censoredXmlList = ApplyJsonXmlDataCensors(xmlList, censorText, elementsToCensors);
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
        private static List<object> ApplyJsonXmlDataCensors(List<object> list, string censorText, IReadOnlyCollection<CensorElement> elementsToCensors)
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
                        var censoredEntryDict = ApplyJsonXmlDataCensors(entryDict, censorText, elementsToCensors);
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
                        var censoredEntryList = ApplyJsonXmlDataCensors(entryList, censorText, elementsToCensors);
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
        private static Dictionary<string, object> ApplyJsonXmlDataCensors(Dictionary<string, object> dictionary, string censorText, IReadOnlyCollection<CensorElement> elementsToCensors)
        {
            if (dictionary.Count == 0)
                // short circuit if there is no data to censor
                return dictionary;

            var censoredBodyDictionary = new Dictionary<string, object>();
            foreach (var elem in dictionary)
            {
                if (ElementShouldBeCensored(foundValue: elem.Value, foundKey: elem.Key, elementsToCensors))
                {
                    var value = dictionary[elem.Key];
                    if (value == null)
                    {
                        // don't need to worry about censoring something that's null (don't replace null with the censor string)
                        continue;
                    }

                    if (Utilities.IsJsonDictionary(value))
                    {
                        // replace with empty dictionary
                        censoredBodyDictionary.Add(elem.Key, new Dictionary<string, object>());
                    }
                    else if (Utilities.IsJsonArray(value))
                    {
                        // replace with empty array
                        censoredBodyDictionary.Add(elem.Key, new List<object>());
                    }
                    else
                    {
                        // replace with censor text
                        censoredBodyDictionary.Add(elem.Key, censorText);
                    }
                }
                else
                {
                    var value = dictionary[elem.Key];

                    if (Utilities.IsJsonDictionary(value))
                    {
                        // recursively censor inner dictionaries
                        var valueDict = ((JObject)dictionary[elem.Key]).ToObject<Dictionary<string, object>>();
                        if (valueDict != null)
                        {
                            // change the value if can be parsed as a dictionary (otherwise, skip censoring)
                            value = ApplyJsonXmlDataCensors(valueDict, censorText, elementsToCensors);
                        }
                    }

                    else if (Utilities.IsJsonArray(value))
                    {
                        // recursively censor list elements
                        var valueList = ((JArray)dictionary[elem.Key]).ToObject<List<object>>();
                        if (valueList != null)
                        {
                            value = ApplyJsonXmlDataCensors(valueList, censorText, elementsToCensors);
                        }
                    }

                    censoredBodyDictionary.Add(elem.Key, value);
                }
            }

            return censoredBodyDictionary;
        }

        /// <summary>
        ///     Check if a JSON element should be censored.
        /// </summary>
        /// <param name="foundValue">The value of the element to evaluate.</param>
        /// <param name="foundKey">The key of the element to evaluate.</param>
        /// <param name="elementsToCensor">A list of elements to censor.</param>
        /// <returns>True if the JSON value should be censored, false otherwise.</returns>
        private static bool ElementShouldBeCensored(object? foundValue, string foundKey, IReadOnlyCollection<CensorElement> elementsToCensor)
        {
            if (!(foundValue is string))
            {
                // short circuit if the value is not a string
                return false;
            }

            return elementsToCensor.Count != 0 && elementsToCensor.Any(element => element.Matches(value: (string)foundValue, key: foundKey));
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
