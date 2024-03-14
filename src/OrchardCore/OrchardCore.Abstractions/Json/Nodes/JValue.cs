namespace System.Text.Json.Nodes;

#nullable enable

public static class JValue
{
    /// <summary>
    /// Parses text representing a single JSON value.
    /// </summary>
    public static JsonValue? Parse(string json) => JNode.Parse(json)?.AsValue();

    /// <summary>
    /// Parses text representing a single JSON value.
    /// </summary>
    public static JsonValue? Parse(string json, JsonNodeOptions? nodeOptions = null, JsonDocumentOptions documentOptions = default)
        => JNode.Parse(json, nodeOptions, documentOptions)?.AsValue();

    /// <summary>
    /// Creates a <see cref="JsonValue"/> from an object.
    /// </summary>
    public static JsonValue? FromObject(object? obj, JsonSerializerOptions? options = null)
    {
        if (obj is JsonValue jsonValue)
        {
            return jsonValue;
        }

        if (obj is JsonElement jsonElement)
        {
            return JsonValue.Create(jsonElement, JOptions.Node);
        }

        return JsonValue.Create(JsonSerializer.SerializeToElement(obj, options ?? JOptions.Default), JOptions.Node);
    }

    /// <summary>
    /// Creates a new instance from an existing <see cref="JsonValue"/>.
    /// </summary>
    public static JsonValue? Clone(this JsonValue? jsonValue) => jsonValue?.DeepClone().AsValue();

    /// <summary>
    /// Gets the raw value of this <see cref="JsonValue"/> without specifying a type.
    /// </summary>
    public static object? GetObjectValue(this JsonValue? jsonValue)
    {
        if (jsonValue is null)
        {
            return null;
        }

        if (!jsonValue.TryGetValue<JsonElement>(out var jsonElement))
        {
            return jsonValue.ToObject<object>();
        }

        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.Null:
                return null;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.True:
                return true;
            case JsonValueKind.String:
                return jsonValue.GetValue<string>();

            case JsonValueKind.Number:
                if (jsonElement.TryGetInt32(out var intValue))
                {
                    return intValue;
                }

                if (jsonValue.TryGetValue<long>(out var longValue))
                {
                    return longValue;
                }

                if (jsonValue.TryGetValue<decimal>(out var decimalValue))
                {
                    return decimalValue;
                }

                if (jsonElement.TryGetBigInteger(out var bigInteger))
                {
                    return bigInteger;
                }

                if (jsonValue.TryGetValue<double>(out var doubleValue))
                {
                    return doubleValue;
                }

                throw new JsonException("Cannot parse number.");

            default:
                throw new JsonException(string.Format("Unknown token type {0}", jsonElement.ValueKind));
        }
    }
}
