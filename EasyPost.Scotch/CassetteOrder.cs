using System;
using System.Collections.Generic;
using System.Linq;
using EasyPost.Scotch.InternalUtilities.JSON.Orders;
using Newtonsoft.Json.Serialization;

namespace EasyPost.Scotch
{
    public static class CassetteOrder
    {
        public enum Direction
        {
            Ascending,
            Descending
        }

        public class None : IOrderOption
        {
            Func<IList<JsonProperty>, IEnumerable<JsonProperty>>? IOrderOption.OrderFunction => null;
        }

        public class Alphabetical : IOrderOption
        {
            private readonly Direction _direction;

            public Alphabetical(Direction direction = Direction.Ascending)
            {
                _direction = direction;
            }

            Func<IList<JsonProperty>, IEnumerable<JsonProperty>>? IOrderOption.OrderFunction =>
                (properties) =>
                {
                    var ordered = properties
                        .OrderBy(p => p.Order ?? int.MaxValue)
                        .ThenBy(p => p.PropertyName);
                    return _direction == Direction.Descending ? ordered.Reverse() : ordered;
                };
        }
    }
}
