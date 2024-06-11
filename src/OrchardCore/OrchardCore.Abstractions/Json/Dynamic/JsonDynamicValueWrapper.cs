namespace System.Text.Json.Dynamic;

public struct JsonDynamicValueWrapper<T>
{
    private readonly T _value;
    private readonly bool _hasValue;

    public JsonDynamicValueWrapper(T value)
    {
        _value = value;
        _hasValue = true;
    }

    public T Value
    {
        get
        {
            if (!_hasValue)
                return default;
            return _value;
        }
    }

    public bool HasValue => _hasValue;

    public static implicit operator JsonDynamicValueWrapper<T>(T value)
    {
        return new JsonDynamicValueWrapper<T>(value);
    }

    public static explicit operator T(JsonDynamicValueWrapper<T> wrapper)
    {
        return wrapper.Value;
    }

    public override string ToString()
    {
        return _hasValue ? _value.ToString() : string.Empty;
    }
}

