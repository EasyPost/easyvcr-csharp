using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace EasyPost.Scotch.InternalUtilities.JSON.Orders
{
    public interface IOrderOption
    {
        internal Func<IList<JsonProperty>, IEnumerable<JsonProperty>>? OrderFunction { get; }
    }
}
