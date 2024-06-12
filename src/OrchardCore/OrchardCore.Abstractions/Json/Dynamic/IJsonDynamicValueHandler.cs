using System.Collections.Generic;
using System.Text.Json.Dynamic;
using System.Text.Json.Nodes;

namespace OrchardCore.Json.Dynamic;
public interface IJsonDynamicValueHandler
{
    int Order { get; }

    /// <summary>
    /// Building dynamic fetch logic for the `ContentItem.Content` property.
    /// <para><see cref="JsonDynamicObject.GetValue(string)"/></para>
    /// <para><seealso cref="DefaultJsonDyanmicValueHandler"/></para>
    /// </summary>
    /// <param name="currentNodeValue"></param>
    /// <param name="memberName"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    bool GetValue(JsonValue currentNodeValue, string memberName, out object result);
}
