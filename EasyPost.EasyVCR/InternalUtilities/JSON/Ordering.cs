using System;
using System.Collections.Generic;
using System.Linq;
using EasyPost.EasyVCR.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EasyPost.EasyVCR.InternalUtilities.JSON
{
    /// <summary>
    ///     Wrapper for custom Newtonsoft.Json contract resolvers
    /// </summary>
    internal static class Ordering
    {
        /// <summary>
        ///     Custom Newtonsoft.Json contract resolver with custom sorting functionality.
        /// </summary>
        internal class OrderedContractResolver : DefaultContractResolver
        {
            private readonly Func<IList<JsonProperty>, IEnumerable<JsonProperty>>? _orderFunc;

            /// <summary>
            ///     Initializes a new instance of the <see cref="OrderedContractResolver" /> class.
            /// </summary>
            /// <param name="orderOption">Order to use when sorting JsonProperty elements.</param>
            internal OrderedContractResolver(IOrderOption orderOption)
            {
                _orderFunc = orderOption.OrderFunction;
            }

            /// <summary>
            ///     Override to provide a custom ordering of properties.
            /// </summary>
            /// <param name="type">The type to create properties for.</param>
            /// <param name="memberSerialization">The member serialization mode for the type.</param>
            /// <returns>Ordered list of JsonProperty objects.</returns>
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var @base = base.CreateProperties(type, memberSerialization);
                if (_orderFunc == null) return @base;

                var ordered = _orderFunc(@base).ToList();
                return ordered;
            }
        }
    }
}
