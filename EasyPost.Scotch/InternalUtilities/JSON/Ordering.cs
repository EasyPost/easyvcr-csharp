using System;
using System.Collections.Generic;
using System.Linq;
using EasyPost.Scotch.InternalUtilities.JSON.Orders;
using Newtonsoft.Json.Serialization;

namespace EasyPost.Scotch.InternalUtilities.JSON
{
    internal static class Orderer
    {
        internal class OrderedContractResolver : DefaultContractResolver
        {
            private readonly Func<IList<JsonProperty>, IEnumerable<JsonProperty>>? _orderFunc;
            
            internal OrderedContractResolver(IOrderOption orderOption)
            {
                _orderFunc = orderOption.OrderFunction;
            }
            
            protected override IList<JsonProperty> CreateProperties(Type type, Newtonsoft.Json.MemberSerialization memberSerialization)
            {
                var @base = base.CreateProperties(type, memberSerialization);
                if (_orderFunc == null)
                {
                    return @base;
                }

                var ordered = _orderFunc(@base).ToList();
                return ordered;
            }
        }
    }
}
