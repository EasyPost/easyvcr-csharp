using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EasyPost.EasyVCR.InternalUtilities.JSON;
using Newtonsoft.Json;

namespace EasyPost.EasyVCR
{
    public sealed class Censors
    {
        private readonly List<string> _bodyParamsToCensor;

        private readonly string _censorText = "*****";
        private readonly List<string> _headersToCensor;
        private readonly List<string> _queryParamsToCensor;

        /// <summary>
        ///     Default censors is to not censor anything.
        /// </summary>
        public static Censors Default => new();

        /// <summary>
        ///     Default sensitive censors is to censor common private information (i.e. API keys, auth tokens, etc.)
        /// </summary>
        public static Censors DefaultSensitive
        {
            get
            {
                var censors = new Censors();
                foreach (var key in Statics.DefaultCredentialHeadersToHide) censors.HideHeader(key);

                foreach (var key in Statics.DefaultCredentialParametersToHide)
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
        public Censors(string? censorString = null)
        {
            _queryParamsToCensor = new List<string>();
            _bodyParamsToCensor = new List<string>();
            _headersToCensor = new List<string>();
            _censorText = censorString ?? _censorText;
        }

        /// <summary>
        ///     Add a rule to censor a specified body parameter.
        ///     Note: Only top-level pairs can be censored.
        /// </summary>
        /// <param name="parameterKey">Key of body parameter to censor.</param>
        /// <returns></returns>
        public Censors HideBodyParameter(string parameterKey)
        {
            _bodyParamsToCensor.Add(parameterKey);
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
            _headersToCensor.Add(headerKey);
            return this;
        }

        /// <summary>
        ///     Add a rule to censor a specified query parameter.
        /// </summary>
        /// <param name="parameterKey">Key of query parameter to censor.</param>
        /// <returns>The current Censor object.</returns>
        public Censors HideQueryParameter(string parameterKey)
        {
            _queryParamsToCensor.Add(parameterKey);
            return this;
        }

        /// <summary>
        ///     Censor the appropriate body parameters.
        /// </summary>
        /// <param name="body">String representation of request body to apply censors to.</param>
        /// <returns>Censored string representation of request body.</returns>
        internal string CensorBodyParameters(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                // short circuit if body is null or empty
                return body;

            Dictionary<string, string> bodyDictionary;
            try
            {
                bodyDictionary = Serialization.ConvertJsonToObject<Dictionary<string, string>>(body);
            }
            catch (JsonSerializationException)
            {
                // short circuit if body is not a JSON dictionary
                return body;
            }

            if (bodyDictionary.Count == 0)
                // short circuit if there are no body parameters
                return body;

            foreach (var key in _bodyParamsToCensor)
                if (bodyDictionary.ContainsKey(key))
                    bodyDictionary[key] = _censorText;

            return Serialization.ConvertObjectToJson(bodyDictionary);
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

            foreach (var key in _headersToCensor)
                if (headers.ContainsKey(key))
                    headers[key] = _censorText;

            return headers;
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

            foreach (var key in _queryParamsToCensor)
                if (queryParameters.AllKeys.Contains(key))
                    queryParameters[key] = _censorText;

            return $"{uri.GetLeftPart(UriPartial.Path)}?{queryParameters}";
        }
    }
}
