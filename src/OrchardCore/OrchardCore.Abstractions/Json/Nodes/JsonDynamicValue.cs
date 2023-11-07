using System.Diagnostics;

#nullable enable

namespace System.Text.Json.Nodes;

[DebuggerDisplay("JsonDynamicValue[{Value}]")]
public class JsonDynamicValue
{
    private readonly object? _value;

    public JsonDynamicValue(JsonValue? jsonValue, object? value = null)
    {
        JsonValue = jsonValue;
        _value = value;
    }

    public JsonValue? JsonValue { get; }

    public object? Value => _value ?? JsonValue.ToObject<object>();
}
