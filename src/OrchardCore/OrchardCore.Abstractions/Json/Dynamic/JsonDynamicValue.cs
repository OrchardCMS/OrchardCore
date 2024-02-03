using System.Text.Json.Nodes;

namespace System.Text.Json.Dynamic;

#nullable enable

public class JsonDynamicValue
{
    private object? _value;
    private bool _hasValue;

    public JsonDynamicValue(JsonValue? jsonValue) => JsonValue = jsonValue;

    public JsonDynamicValue(JsonValue? jsonValue, object? value)
    {
        JsonValue = jsonValue;
        _value = value;
    }

    public JsonValue? JsonValue { get; }

    public object? Value
    {
        get
        {
            if (!_hasValue)
            {
                _value = JsonValue?.GetObjectValue();
                _hasValue = true;
            }

            return _value;
        }
    }
}
