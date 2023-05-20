using System;
using System.Text.RegularExpressions;

namespace EasyVCR
{
    /// <summary>
    ///     The base class for censor elements.
    /// </summary>
    public abstract class CensorElement
    {
        /// <summary>
        ///     Whether the name is case-sensitive.
        /// </summary>
        protected bool CaseSensitive { get; }

        /// <summary>
        ///     Value to look for.
        /// </summary>
        protected string Value { get; }

        /// <summary>
        ///     Constructor for a new censor element.
        /// </summary>
        /// <param name="value">Value to censor.</param>
        /// <param name="caseSensitive">Whether the value is case-sensitive.</param>
        protected CensorElement(string value, bool caseSensitive)
        {
            Value = value;
            CaseSensitive = caseSensitive;
        }

        /// <summary>
        ///     Checks whether the provided element matches this censor element, accounting for case sensitivity.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the element matches, false otherwise.</returns>
        internal abstract bool Matches(string value, string? key = null);
    }

    /// <summary>
    ///     A censor element, used to define a raw value that should be censored.
    /// </summary>
    public class TextCensorElement : CensorElement
    {
        /// <summary>
        ///     Constructor for a new text censor element.
        /// </summary>
        /// <param name="value">The raw text value of the element to censor.</param>
        /// <param name="caseSensitive">Whether the value is case-sensitive.</param>
        public TextCensorElement(string value, bool caseSensitive) : base(value, caseSensitive)
        {
        }

        /// <summary>
        ///     Checks whether the provided element matches this censor element, accounting for case sensitivity.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the element matches, false otherwise.</returns>
        internal override bool Matches(string value, string? key = null)
        {
            // we only care about the value here
            return CaseSensitive ? Value.Equals(value) : Value.Equals(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Replace the provided value with the provided replacement if it matches this censor element.
        ///     Otherwise, return the value as-is.
        /// </summary>
        /// <param name="value">Value to replace.</param>
        /// <param name="replacement">Replacement for the value.</param>
        /// <returns>The value with the replacement inserted if it matches this censor element, otherwise the value as-is.</returns>
        internal string MatchAndReplaceAsNeeded(string value, string replacement)
        {
            return Matches(value) ? replacement : value;
        }
    }

    /// <summary>
    ///     A censor element, used to define a key-value pair that should be censored.
    /// </summary>
    public class KeyCensorElement : CensorElement
    {
        /// <summary>
        ///     Constructor for a new key censor element.
        /// </summary>
        /// <param name="key">Key of which to censor the corresponding value.</param>
        /// <param name="caseSensitive">Whether the key is case-sensitive.</param>
        public KeyCensorElement(string key, bool caseSensitive) : base(key, caseSensitive)
        {
        }

        /// <summary>
        ///     Checks whether the provided element matches this censor element, accounting for case sensitivity.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the element matches, false otherwise.</returns>
        internal override bool Matches(string value, string? key = null)
        {
            // we only care about the key here
            return CaseSensitive ? Value.Equals(key) : Value.Equals(key, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    ///     A censor element, used to define a regex pattern that should be censored.
    /// </summary>
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

            return Regex.Replace(
                value,
                Value,
                replacement,
                options,
                TimeSpan.FromMilliseconds(250)
            );
        }

        /// <summary>
        ///     Checks whether the provided element matches this censor element, accounting for case sensitivity.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the element matches, false otherwise.</returns>
        internal override bool Matches(string value, string? key = null)
        {
            // we only care about the value here
            var options = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Singleline;
            if (!CaseSensitive)
            {
                options |= RegexOptions.IgnoreCase;
            }

            try
            {
                return Regex.IsMatch(
                    value,
                    Value,
                    options,
                    TimeSpan.FromMilliseconds(250)
                    );
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}
