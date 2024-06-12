using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;

namespace OrchardCore.Json.Dynamic;
public class DefaultJsonDyanmicValueHandler : IJsonDynamicValueHandler
{
    public bool GetValue(JsonObject jsonObject, Dictionary<string, object> dynamicValueDict, string memberName, JsonNode currentNode)
    {
        if (currentNode is JsonValue jsonValue)
        {
            var valueKind = jsonValue.GetValueKind();
            switch (valueKind)
            {
                case JsonValueKind.String:
                    if (memberName == "Value")
                    {
                        if (jsonValue.TryGetValue<DateTime>(out var datetime))
                        {
                            dynamicValueDict[memberName] = datetime;
                            return true;
                        }

                        if (jsonValue.TryGetValue<TimeSpan>(out var timeSpan))
                        {
                            dynamicValueDict[memberName] = timeSpan;
                            return true;
                        }
                    }
                    dynamicValueDict[memberName] = jsonValue.GetString();
                    return true;
                case JsonValueKind.Number:
                    dynamicValueDict[memberName] = jsonValue.GetNumber();
                    return true;
                case JsonValueKind.True:
                    dynamicValueDict[memberName] = true;
                    return true;
                case JsonValueKind.False:
                    dynamicValueDict[memberName] = false;
                    return true;
            }
        }
        return false;
    }
}
