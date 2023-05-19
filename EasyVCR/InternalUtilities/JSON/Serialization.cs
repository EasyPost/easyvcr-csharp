using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Xml.Linq;
using EasyVCR.Interfaces;
using Newtonsoft.Json;

// ReSharper disable MemberCanBePrivate.Global

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
        /// <exception cref="Exception">Deserialization failed.</exception>
        internal static T ConvertJsonToObject<T>(string? data, params JsonConverter[] converters)
        {
            if (data == null || string.IsNullOrWhiteSpace(data)) throw new Exception("No data to deserialize");
            try
            {
                return JsonConvert.DeserializeObject<T>(data, converters) ?? throw new Exception("Deserialization failed");
            }
            catch (Exception e)
            {
                throw new Exception("Deserialization failed", e);
            }
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
        ///     Convert a JSON string to an XML string
        /// </summary>
        /// <param name="json">JSON string to convert to XML.</param>
        /// <returns>An XML string.</returns>
        internal static string? ConvertJsonToXml(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }
            
            XNode? node = JsonConvert.DeserializeXNode(json);
            return node?.ToString();
        }

        /// <summary>
        ///     Convert an object to a JSON string
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <param name="formatting">Formatting setting to use in final JSON string.</param>
        /// <param name="converters">JsonConverters to use during serialization.</param>
        /// <returns>JSON string of object.</returns>
        internal static string ConvertObjectToJson(object obj, Formatting formatting = Formatting.None, params JsonConverter[] converters)
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
        internal static string ConvertObjectToJson(object obj, IOrderOption orderOption, Formatting formatting = Formatting.None, params JsonConverter[] converters)
        {
            if (obj == null) throw new Exception("No object to serialize");

            // modify settings so elements will be ordered
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new Ordering.OrderedContractResolver(orderOption),
                Converters = converters,
                NullValueHandling = NullValueHandling.Ignore
            };

            return JsonConvert.SerializeObject(obj, formatting, settings);
        }

        /// <summary>
        ///     Convert an XML string to a JSON string
        /// </summary>
        /// <param name="xml">XML string to convert to JSON.</param>
        /// <returns>A JSON string.</returns>
        internal static string ConvertXmlToJson(string xml)
        {
            var doc = XDocument.Parse(xml); //or XDocument.Load(path)
            return JsonConvert.SerializeXNode(doc);
        }

        /// <summary>
        ///     Normalize a JSON string to remove CRLF and other whitespace.
        /// </summary>
        /// <param name="json">JSON string to normalize.</param>
        /// <param name="removeElements">List of elements to remove from the JSON string.</param>
        /// <returns>Normalized JSON string.</returns>
        internal static string? NormalizeJson(string json, List<CensorElement>? removeElements = null)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            // need to use censors to remove elements from the JSON string
            if (removeElements != null)
            {
                return Censors.CensorJsonData(json, "FILTERED", removeElements);
            }

            // don't need to remove elements
            object obj = ConvertJsonToObject(json);
            return ConvertObjectToJson(obj);
        }
    }
}
