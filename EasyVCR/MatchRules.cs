using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EasyVCR.RequestElements;
using JsonSerialization = EasyVCR.InternalUtilities.JSON.Serialization;
using XmlSerialization = EasyVCR.InternalUtilities.XML.Serialization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global

namespace EasyVCR
{
    /// <summary>
    ///     Rule set for matching requests against recorded requests.
    /// </summary>
    public class MatchRules
    {
        private readonly List<Func<Request, Request, bool>> _rules;

        /// <summary>
        ///     Default rule is to match on the method and URL.
        /// </summary>
        public static MatchRules Default => new MatchRules().ByMethod().ByFullUrl();

        /// <summary>
        ///     Default strict rule is to match on the method, URL and body.
        /// </summary>
        public static MatchRules DefaultStrict => new MatchRules().ByMethod().ByFullUrl().ByBody();

        /// <summary>
        ///     Initialize a new instance of the <see cref="MatchRules" /> factory.
        /// </summary>
        public MatchRules()
        {
            _rules = new List<Func<Request, Request, bool>>();
        }
        
        /// <summary>
        ///     Add a rule to compare the HTTP methods of the requests.
        /// </summary>
        /// <returns>The same MatchRules object.</returns>
        public MatchRules ByMethod()
        {
            By((received, recorded) => received.Method.Equals(recorded.Method, StringComparison.OrdinalIgnoreCase));
            return this;
        }

        /// <summary>
        ///     Add a rule to compare the base URLs of the requests.
        ///     Ex. http://example.com in http://example.com/path/to/resource?query=string
        /// </summary>
        /// <returns>The same MatchRules object.</returns>
        public MatchRules ByBaseUrl()
        {
            By((received, recorded) =>
            {
                var receivedUri = new Uri(received.Uri ?? string.Empty).GetLeftPart(UriPartial.Path);
                var recordedUri = new Uri(recorded.Uri ?? string.Empty).GetLeftPart(UriPartial.Path);
                return receivedUri.Equals(recordedUri, StringComparison.InvariantCultureIgnoreCase);
            });
            return this;
        }
        
        /// <summary>
        ///     Add a rule to compare the paths of the requests.
        ///     Ex. /path/to/resource in http://example.com/path/to/resource?query=string
        /// </summary>
        /// <returns>The same MatchRules object.</returns>
        public MatchRules ByPath()
        {
            By((received, recorded) =>
            {
                var receivedPath = new Uri(received.Uri ?? string.Empty).AbsolutePath;
                var recordedPath = new Uri(recorded.Uri ?? string.Empty).AbsolutePath;
                return receivedPath.Equals(recordedPath, StringComparison.OrdinalIgnoreCase);
            });
            return this;
        }

        /// <summary>
        ///     Add a rule to compare the query parameters of the requests.
        ///     Ex. query=string in http://example.com/path/to/resource?query=string
        /// </summary>
        /// <param name="exact">
        ///     If true, query parameters must be in the same exact order to match. If false, query parameter order
        ///     doesn't matter.
        /// </param>
        /// <returns>The same MatchRules object.</returns>
        public MatchRules ByQuery(bool exact = false)
        {
            By((received, recorded) =>
            {
                var receivedQuery = new Uri(received.Uri ?? string.Empty).Query;
                var recordedQuery = new Uri(recorded.Uri ?? string.Empty).Query;
                var receivedQueryDict = HttpUtility.ParseQueryString(receivedQuery);
                var recordedQueryDict = HttpUtility.ParseQueryString(recordedQuery);
                return receivedQueryDict.Count == recordedQueryDict.Count && receivedQueryDict.AllKeys.All(key => receivedQueryDict[key] == recordedQueryDict[key]);
            });
            return this;
        }
        
        /// <summary>
        ///     Add a rule to compare the full URLs (including query parameters) of the requests.
        ///     Ex. http://example.com/path/to/resource?query=string in http://example.com/path/to/resource?query=string
        /// </summary>
        /// <param name="exact">
        ///     If true, query parameters must be in the same exact order to match. If false, query parameter order
        ///     doesn't matter.
        /// </param>
        /// <returns>The same MatchRules object.</returns>
        public MatchRules ByFullUrl(bool exact = false)
        {
            ByBaseUrl();
            ByPath();
            ByQuery(exact: exact);
            return this;
        }

        /// <summary>
        ///     Add a rule to compare the bodies of the requests.
        /// </summary>
        /// <param name="ignoredElements">List of body elements to ignore when matching by body.</param>
        /// <returns>The same MatchRules object.</returns>
        public MatchRules ByBody(List<CensorElement>? ignoredElements = null)
        {
            By((received, recorded) =>
            {
                if (received.Body == null && recorded.Body == null)
                    // both have null bodies, so they match
                    return true;

                if (received.Body == null || recorded.Body == null)
                    // one has a null body, so they don't match
                    return false;

                var receivedBody = received.Body;
                var recordedBody = recorded.Body;
                try
                {
                    receivedBody = JsonSerialization.NormalizeJson(received.Body, ignoredElements);
                }
                catch (Exception)
                {
                    // not JSON, using the string as it is
                }

                try
                {
                    recordedBody = JsonSerialization.NormalizeJson(recorded.Body, ignoredElements);
                }
                catch (Exception)
                {
                    // not JSON, using the string as it is
                }

                if (receivedBody == null && recordedBody == null)
                    // both have empty string bodies, so they match
                    return true;

                return receivedBody!.Equals(recordedBody, StringComparison.OrdinalIgnoreCase);
            });
            return this;
        }
        
        /// <summary>
        ///     Add a rule to compare a specific header of the requests.
        /// </summary>
        /// <param name="name">Key of the header to compare.</param>
        /// <returns>The same MatchRules object.</returns>
        public MatchRules ByHeader(string name)
        {
            By((received, recorded) =>
            {
                var receivedHeaders = received.RequestHeaders;
                var recordedHeaders = recorded.RequestHeaders;
                if (!receivedHeaders.ContainsKey(name)) return !recordedHeaders.ContainsKey(name);

                if (!recordedHeaders.ContainsKey(name)) return false;

                var receivedHeader = receivedHeaders[name];
                var recordedHeader = recordedHeaders[name];
                return receivedHeader.Equals(recordedHeader, StringComparison.OrdinalIgnoreCase);
            });
            return this;
        }

        /// <summary>
        ///     Add a rule to compare the headers of the requests.
        /// </summary>
        /// <param name="exact">
        ///     If true, both requests must have the exact same headers.
        ///     If false, as long as the evaluated request has all the headers of the matching request (and potentially more), the
        ///     match is considered valid.
        /// </param>
        /// <returns>The same MatchRules object.</returns>
        public MatchRules ByHeaders(bool exact = false)
        {
            if (exact)
                // first, we'll check that there are the same number of headers in both requests. If they're are, then the second check is guaranteed to compare all headers.
                By((received, recorded) => received.RequestHeaders.Count == recorded.RequestHeaders.Count);

            By((received, recorded) =>
            {
                var receivedHeaders = received.RequestHeaders;
                var recordedHeaders = recorded.RequestHeaders;
                return receivedHeaders.All(h => recordedHeaders.Contains(h));
            });
            return this;
        }

        /// <summary>
        ///     Add a custom rule to compare two requests.
        /// </summary>
        /// <param name="rule">
        ///     A <see cref="Func{Request, Request, TResult}"/> that takes in two <see cref="Request"/> objects and outputs a boolean.
        ///     The first <see cref="Request"/> is the current pre-flight request, and the second is a previously-recorded request from the current cassette.
        /// </param>
        /// <returns>The same MatchRules object.</returns>
        public MatchRules ByCustomRule(Func<Request, Request, bool> rule)
        {
            By(rule);
            return this;
        }
        
        /// <summary>
        ///     Add a rule to compare the entire requests.
        ///     Note, this rule is very strict, and will fail if the requests are not identical (including duration).
        ///     It is highly recommended to use the other rules to compare the requests.
        /// </summary>
        /// <returns>The same MatchRules object.</returns>
        public MatchRules ByEverything()
        {
            By((received, recorded) =>
            {
                var receivedRequest = received.ToJson();
                var recordedRequest = recorded.ToJson();
                return receivedRequest.Equals(recordedRequest, StringComparison.OrdinalIgnoreCase);
            });
            return this;
        }


        /// <summary>
        ///     Execute rules to determine if the received request matches the recorded request
        /// </summary>
        /// <param name="receivedRequest">Request to find a match for.</param>
        /// <param name="recordedRequest">Request to compare against.</param>
        /// <returns>True if the requests match, false otherwise.</returns>
        internal bool RequestsMatch(Request receivedRequest, Request recordedRequest)
        {
            return _rules.Count == 0 || _rules.All(rule => rule(receivedRequest, recordedRequest));
        }

        /// <summary>
        ///     Add a rule to the list of rules to match
        /// </summary>
        /// <param name="rule">Rule to add</param>
        private void By(Func<Request, Request, bool> rule)
        {
            _rules.Add(rule);
        }
    }
}
