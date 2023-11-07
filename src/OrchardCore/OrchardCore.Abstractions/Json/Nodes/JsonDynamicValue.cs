using System.Diagnostics;

#nullable enable

namespace System.Text.Json.Nodes;

[DebuggerDisplay("JsonDynamicValue[{Value}]")]
public class JsonDynamicValue
{
    public JsonDynamicValue(JsonValue? jsonValue, object? value = null)
    {
        JsonValue = jsonValue;
        Value = value ?? JsonValue.ToObject<object>();
    }

    public JsonValue? JsonValue { get; }

    public object? Value { get; }
}
