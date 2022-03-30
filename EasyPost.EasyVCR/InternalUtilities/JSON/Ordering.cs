using System;
using System.Collections.Generic;
using System.Linq;
using EasyPost.EasyVCR.InternalUtilities.JSON.Orders;
using Newtonsoft.Json.Serialization;

namespace EasyPost.EasyVCR.InternalUtilities.JSON
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
            
            /// <summary>
            /// Override to provide a custom ordering of properties.
            /// </summary>
            /// <param name="type">The type to create properties for.</param>
            /// <param name="memberSerialization">The member serialization mode for the type.</param>
            /// <returns>Ordered list of JsonProperty objects.</returns>
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
