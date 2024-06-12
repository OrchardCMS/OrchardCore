using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;

namespace OrchardCore.Json.Dynamic;
public class DefaultJsonDyanmicValueHandler : IJsonDynamicValueHandler
{
    public int Order => 0;

    public bool GetValue(JsonValue currentNodeValue, string memberName, out object result)
    {
        var valueKind = currentNodeValue.GetValueKind();
        switch (valueKind)
        {
            case JsonValueKind.String:
                if (memberName == "Value")
                {
                    if (currentNodeValue.TryGetValue<DateTime>(out var datetime))
                    {
                        result = datetime;
                        return true;
                    }

                    if (currentNodeValue.TryGetValue<TimeSpan>(out var timeSpan))
                    {
                        result = timeSpan;
                        return true;
                    }
                }

                result = currentNodeValue.GetString();
                return true;
            case JsonValueKind.Number:
                result = currentNodeValue.GetNumber();
                return true;
            case JsonValueKind.True:
                result = true;
                return true;
            case JsonValueKind.False:
                result = false;
                return true;
        }

        result = null;
        return false;
    }
}
