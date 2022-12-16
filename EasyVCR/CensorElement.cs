using System;
using System.Text.RegularExpressions;

namespace EasyVCR
{
    public class CensorElement
    {
        /// <summary>
        ///     Whether the name is case-sensitive.
        /// </summary>
        protected bool CaseSensitive { get; }
        /// <summary>
        ///     Name of the element to censor.
        /// </summary>
        protected string Value { get; }

        /// <summary>
        ///     Constructor for a new censor element.
        /// </summary>
        /// <param name="value">Name of the element to censor.</param>
        /// <param name="caseSensitive">Whether the name is case-sensitive.</param>
        public CensorElement(string value, bool caseSensitive)
        {
            Value = value;
            CaseSensitive = caseSensitive;
        }

        /// <summary>
        ///     Checks whether the provided element matches this censor element, accounting for case sensitivity.
        /// </summary>
        /// <param name="key">The name to check.</param>
        /// <returns>True if the element matches, false otherwise.</returns>
        internal virtual bool Matches(string key)
        {
            return CaseSensitive ? Value.Equals(key) : Value.Equals(key, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class RegexCensorElement : CensorElement
    {
        /// <summary>
        ///     Constructor for a new regex censor element.
        /// </summary>
        /// <param name="pattern">Pattern of the element to censor.</param>
        /// <param name="caseSensitive">Whether the pattern is case-sensitive.</param>
        public RegexCensorElement(string pattern, bool caseSensitive) : base(pattern, caseSensitive)
        {
        }
        
        /// <summary>
        ///     Replace the provided value with the provided replacement if the value matches the regex pattern.
        ///     Returns the original value if the value does not match the regex pattern.
        /// </summary>
        /// <param name="value">Value to apply the replacement to.</param>
        /// <param name="replacement">Replacement for a detected matching section.</param>
        /// <returns>The value with the replacement inserted, or the original value if no match was found.</returns>
        internal string MatchAndReplaceAsNeeded(string value, string replacement)
        {
            var options = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline;
            if (!CaseSensitive)
            {
                options |= RegexOptions.IgnoreCase;
            }
            
            return Regex.Replace(value,
                Value,
                replacement,
                options,
                TimeSpan.FromMilliseconds(250));
        }
        
        /// <summary>
        ///     Checks whether the provided element matches this censor element, accounting for case sensitivity.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the element matches, false otherwise.</returns>
        internal override bool Matches(string value)
        {
            var options = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline;
            if (!CaseSensitive)
            {
                options |= RegexOptions.IgnoreCase;
            }

            try
            {
                return Regex.IsMatch(value,
                    Value,
                    options,
                    TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}
