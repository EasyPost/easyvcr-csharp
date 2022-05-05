using System;
using System.Dynamic;
using JsonSerialization = EasyVCR.InternalUtilities.JSON.Serialization;

namespace EasyVCR.InternalUtilities.XML
{
    /// <summary>
    ///     XML de/serialization utilities
    /// </summary>
    internal static class Serialization
    {
        /// <summary>
        ///     Convert an object to an XML string
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <returns>XML string of object.</returns>
        /// <exception cref="Exception">Serialization failed.</exception>
        internal static string ConvertObjectToXml(object obj)
        {
            if (obj == null) throw new Exception("No object to serialize");

            try
            {
                // we convert the object to JSON, then from JSON to XML
                var json = JsonSerialization.ConvertObjectToJson(obj);
                var xml = JsonSerialization.ConvertJsonToXml(json);
                if (xml == null) throw new Exception("Serialization failed");
                return xml;
            }
            catch (Exception e)
            {
                throw new Exception("Serialization failed", e);
            }
        }

        /// <summary>
        ///     Convert an XML string to an object
        /// </summary>
        /// <param name="data">XML string to deserialize.</param>
        /// <typeparam name="T">Type of object to deserialize to.</typeparam>
        /// <returns>A T-type object.</returns>
        /// <exception cref="Exception">Deserialization failed.</exception>
        internal static T ConvertXmlToObject<T>(string? data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data)) throw new Exception("No data to deserialize");
            try
            {
                // we convert the XML to JSON, then from JSON to an object
                var json = JsonSerialization.ConvertXmlToJson(data);
                return JsonSerialization.ConvertJsonToObject<T>(json);
            }
            catch (Exception e)
            {
                throw new Exception("Deserialization failed", e);
            }
        }

        /// <summary>
        ///     Convert an XML string to an ExpandoObject object
        /// </summary>
        /// <param name="data">XML string to deserialize.</param>
        /// <returns>An ExpandoObject object.</returns>
        internal static ExpandoObject ConvertXmlToObject(string? data)
        {
            return ConvertXmlToObject<ExpandoObject>(data);
        }



        /// <summary>
        ///     Normalize an XML string to remove CRLF and other whitespace.
        /// </summary>
        /// <param name="xml">XML string to normalize.</param>
        /// <returns>Normalized XML string.</returns>
        internal static string? NormalizeXml(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                return null;
            }

            object obj = ConvertXmlToObject(xml);
            return ConvertObjectToXml(obj);
        }
    }
}
