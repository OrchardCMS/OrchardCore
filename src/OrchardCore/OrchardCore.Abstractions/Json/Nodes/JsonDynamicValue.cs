#nullable enable

namespace System.Text.Json.Nodes;

public class JsonDynamicValue
{
    private object? _value;
    private bool _initialized;

    public JsonDynamicValue(JsonValue? jsonValue, object? value = null)
    {
        JsonValue = jsonValue;
        _value = value;
    }

    public JsonValue? JsonValue { get; }

    public object? Value
    {
        get
        {
            if (!_initialized)
            {
                _value ??= JsonValue.ToObject<object>();
                _initialized = true;
            }

            return _value;
        }
    }
}
