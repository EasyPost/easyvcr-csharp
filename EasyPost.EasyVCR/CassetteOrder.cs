using System;
using System.Collections.Generic;
using System.Linq;
using EasyPost.EasyVCR.InternalUtilities.JSON.Orders;
using Newtonsoft.Json.Serialization;

namespace EasyPost.EasyVCR
{
    public static class CassetteOrder
    {
        /// <summary>
        ///     Direction of the order.
        /// </summary>
        public enum Direction
        {
            Ascending,
            Descending
        }

        /// <summary>
        ///     Organize elements in the cassette in no particular order.
        /// </summary>
        public class None : IOrderOption
        {
            /// <summary>
            ///     Function to order the JSON elements in the cassette in no order.
            /// </summary>
            Func<IList<JsonProperty>, IEnumerable<JsonProperty>>? IOrderOption.OrderFunction => null;
        }

        /// <summary>
        ///     Organize elements in the cassette in alphabetical order.
        /// </summary>
        public class Alphabetical : IOrderOption
        {
            private readonly Direction _direction;

            /// <summary>
            ///     Function to order the JSON elements in the cassette in alphabetical order.
            /// </summary>
            Func<IList<JsonProperty>, IEnumerable<JsonProperty>>? IOrderOption.OrderFunction =>
                (properties) =>
                {
                    var ordered = properties
                        .OrderBy(p => p.Order ?? int.MaxValue)
                        .ThenBy(p => p.PropertyName);
                    return _direction == Direction.Descending ? ordered.Reverse() : ordered;
                };

            public Alphabetical(Direction direction = Direction.Ascending)
            {
                _direction = direction;
            }
        }
    }
}
