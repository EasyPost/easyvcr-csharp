using System;

namespace EasyVCR
{
    public class CensorElement
    {
        /// <summary>
        ///     Whether the name is case-sensitive.
        /// </summary>
        private bool CaseSensitive { get; set; }
        /// <summary>
        ///     Name of the element to censor.
        /// </summary>
        private string Name { get; set; }

        /// <summary>
        ///     Constructor for a new censor element.
        /// </summary>
        /// <param name="name">Name of the element to censor.</param>
        /// <param name="caseSensitive">Whether the name is case-sensitive.</param>
        public CensorElement(string name, bool caseSensitive)
        {
            Name = name;
            CaseSensitive = caseSensitive;
        }

        /// <summary>
        ///     Checks whether the provided element matches this censor element, accounting for case sensitivity.
        /// </summary>
        /// <param name="key">The name to check.</param>
        /// <returns>True if the element matches, false otherwise.</returns>
        internal bool Matches(string key)
        {
            return CaseSensitive ? Name.Equals(key) : Name.Equals(key, StringComparison.OrdinalIgnoreCase);
        }
    }
}
