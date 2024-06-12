using System.Collections.Generic;
using System.Text.Json.Dynamic;
using System.Text.Json.Nodes;

namespace OrchardCore.Json.Dynamic;
public interface IJsonDynamicValueHandler
{
    /// <summary>
    /// Building dynamic fetch logic for the ContentItem.Content property.
    /// <para><see cref="JsonDynamicObject.GetValue(string)"/></para>
    /// <para><seealso cref="DefaultJsonDyanmicValueHandler"/></para>
    /// </summary>
    /// <param name="parentNode"></param>
    /// <param name="dynamicValueDict"></param>
    /// <param name="memberName"></param>
    /// <param name="memberNode"></param>
    /// <returns></returns>
    bool GetValue(JsonObject parentNode, Dictionary<string, object?> dynamicValueDict, string memberName, JsonNode memberNode);
}
