using System;
using System.Dynamic;
using EasyVCR.Interfaces;
using Newtonsoft.Json;

namespace EasyVCR.InternalUtilities.JSON
{
    /// <summary>
    ///     JSON de/serialization utilities
    /// </summary>
    internal static class Serialization
    {
        /// <summary>
        ///     Convert a JSON string to an object
        /// </summary>
        /// <param name="data">JSON string to deserialize.</param>
        /// <param name="converters">JsonConverters to use during deserialization.</param>
        /// <typeparam name="T">Type of object to deserialize to.</typeparam>
        /// <returns>A T-type object.</returns>
        /// <exception cref="Exception">No JSON data to deserialize.</exception>
        internal static T ConvertJsonToObject<T>(string? data, params JsonConverter[] converters)
        {
            if (data == null || string.IsNullOrWhiteSpace(data)) throw new Exception("No data to deserialize");

            return JsonConvert.DeserializeObject<T>(data, converters) ?? throw new Exception("Deserialization failed");
        }

        /// <summary>
        ///     Convert a JSON string to an ExpandoObject object
        /// </summary>
        /// <param name="data">JSON string to deserialize.</param>
        /// <param name="converters">JsonConverters to use during deserialization.</param>
        /// <returns>An ExpandoObject object.</returns>
        internal static ExpandoObject ConvertJsonToObject(string? data, params JsonConverter[] converters)
        {
            return ConvertJsonToObject<ExpandoObject>(data, converters);
        }

        /// <summary>
        ///     Convert an object to a JSON string
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <param name="formatting">Formatting setting to use in final JSON string.</param>
        /// <param name="converters">JsonConverters to use during serialization.</param>
        /// <returns>JSON string of object.</returns>
        internal static string ConvertObjectToJson(object obj, Formatting formatting = Formatting.Indented, params JsonConverter[] converters)
        {
            return ConvertObjectToJson(obj, new CassetteOrder.None(), formatting, converters);
        }

        /// <summary>
        ///     Convert an object to a JSON string
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <param name="orderOption">Order to organize JSON elements.</param>
        /// <param name="formatting">Formatting setting to use in final JSON string.</param>
        /// <param name="converters">JsonConverters to use during serialization.</param>
        /// <returns>JSON string of object.</returns>
        /// <exception cref="Exception">No object to serialize.</exception>
        internal static string ConvertObjectToJson(object obj, IOrderOption orderOption, Formatting formatting = Formatting.Indented, params JsonConverter[] converters)
        {
            if (obj == null) throw new Exception("No object to serialize");

            // modify settings so elements will be ordered
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new Ordering.OrderedContractResolver(orderOption),
                Converters = converters
            };

            return JsonConvert.SerializeObject(obj, formatting, settings);
        }
    }
}
