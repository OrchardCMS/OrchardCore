using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace OrchardCore.Json.Dynamic;
public interface IJsonDynamicValueHandler
{
    bool GetValue(JsonObject jsonObject, Dictionary<string, object?> dynamicValueDict, string memberName, JsonNode currentNode);
}
