using System;
using System.Dynamic;
using EasyPost.Scotch.InternalUtilities.JSON.Orders;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EasyPost.Scotch.InternalUtilities.JSON
{
    /// <summary>
    /// JSON de/serialization utilities
    /// </summary>
    internal static class Serialization
    {
        internal static T ConvertJsonToObject<T>(string? data, params JsonConverter[] converters)
        {
            if (data == null || string.IsNullOrWhiteSpace(data))
            {
                throw new Exception("No data to deserialize");
            }

            return JsonConvert.DeserializeObject<T>(data, converters) ?? throw new Exception("Deserialization failed");
        }

        internal static ExpandoObject ConvertJsonToObject(string? data, params JsonConverter[] converters) => ConvertJsonToObject<ExpandoObject>(data, converters);
        
        internal static string ConvertObjectToJson(object obj, Formatting formatting = Formatting.Indented, params JsonConverter[] converters)
        {
            return ConvertObjectToJson(obj, new CassetteOrder.None(), formatting, converters);
        }
        
        internal static string ConvertObjectToJson(object obj, IOrderOption orderOption, Formatting formatting = Formatting.Indented, params JsonConverter[] converters)
        {
            // modify settings so elements will be ordered
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new Orderer.OrderedContractResolver(orderOption),
                Converters = converters
            };
            
            if (obj == null)
            {
                throw new Exception("No object to serialize");
            }
            
            return JsonConvert.SerializeObject(obj, formatting, settings);
        }
    }
    
    
}
