using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace EasyPost.EasyVCR.Interfaces
{
    /// <summary>
    ///     Interface for a custom JSON element order option
    /// </summary>
    public interface IOrderOption
    {
        /// <summary>
        ///     Function to order the JsonProperty objects.
        /// </summary>
        internal Func<IList<JsonProperty>, IEnumerable<JsonProperty>>? OrderFunction { get; }
    }
}
