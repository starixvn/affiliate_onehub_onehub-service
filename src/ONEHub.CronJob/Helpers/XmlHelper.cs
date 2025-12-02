using System.Xml.Linq;

namespace ONEHub.CronJob.Helpers;

/// <summary>
/// Helper class for building XML data for batch imports
/// </summary>
public static class XmlHelper
{
    /// <summary>
    /// Builds XML string from collection of objects
    /// </summary>
    /// <typeparam name="T">Type of objects to serialize</typeparam>
    /// <param name="data">Collection of objects</param>
    /// <param name="rootElementName">Root element name (default: "root")</param>
    /// <param name="itemElementName">Item element name (default: "row")</param>
    /// <returns>XML string</returns>
    public static string BuildXml<T>(IEnumerable<T> data, string rootElementName = "root", string itemElementName = "row")
    {
        if (!data.Any())
        {
            return $"<{rootElementName}></{rootElementName}>";
        }

        var xml = new XElement(rootElementName);
        var properties = typeof(T).GetProperties();

        foreach (var item in data)
        {
            var rowElement = new XElement(itemElementName);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(item);
                if (value != null)
                {
                    rowElement.Add(new XAttribute(prop.Name.ToLowerInvariant(), value));
                }
            }

            xml.Add(rowElement);
        }

        return xml.ToString(SaveOptions.DisableFormatting);
    }

    /// <summary>
    /// Builds XML string with custom attribute mapping
    /// </summary>
    public static string BuildXmlWithMapping<T>(
        IEnumerable<T> data,
        Dictionary<string, Func<T, object?>> attributeMapping,
        string rootElementName = "root",
        string itemElementName = "row")
    {
        if (!data.Any())
        {
            return $"<{rootElementName}></{rootElementName}>";
        }

        var xml = new XElement(rootElementName);

        foreach (var item in data)
        {
            var rowElement = new XElement(itemElementName);

            foreach (var mapping in attributeMapping)
            {
                var value = mapping.Value(item);
                if (value != null)
                {
                    rowElement.Add(new XAttribute(mapping.Key, value));
                }
            }

            xml.Add(rowElement);
        }

        return xml.ToString(SaveOptions.DisableFormatting);
    }
}

