using System;
using JsonSerialization = EasyVCR.InternalUtilities.JSON.Serialization;
using XmlSerialization = EasyVCR.InternalUtilities.XML.Serialization;

namespace EasyVCR.InternalUtilities
{
    internal enum ContentType
    {
        Json,
        Xml,
        Html,
        Text
    }


    internal static class ContentTypeExtensions
    {
        public static ContentType DetermineContentType(string content)
        {
            if (IsJson(content))
            {
                return ContentType.Json;
            }

            // Need to check HTML first, as HTML is also valid XML
            if (IsHtml(content))
            {
                return ContentType.Html;
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (IsXml(content))
            {
                return ContentType.Xml;
            }

            return ContentType.Text;
        }

        public static ContentType? FromString(string? contentType)
        {
            if (contentType == null) return null;
            return contentType.ToLower() switch
            {
                "json" => ContentType.Json,
                "xml" => ContentType.Xml,
                "html" => ContentType.Html,
                var _ => ContentType.Text
            };
        }

        private static bool IsHtml(string content)
        {
            return content.ToLower().Contains("<html");
        }

        private static bool IsJson(string content)
        {
            try
            {
                // try to serialize the string as JSON to an object
                JsonSerialization.ConvertJsonToObject<object>(content);
                return true;
            }
            catch (Exception)
            {
                // if it fails, it's not JSON
                return false;
            }
        }

        private static bool IsXml(string content)
        {
            try
            {
                // try to serialize the string as XML to an object
                XmlSerialization.ConvertXmlToObject<object>(content);
                return true;
            }
            catch (Exception)
            {
                // if it fails, it's not XML
                return false;
            }
        }
    }
}
