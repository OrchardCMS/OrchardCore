namespace System.Text.Json.Nodes;

#nullable enable

public static class JValue
{
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
            return JsonValue.Create(jsonElement);
        }

        return JsonValue.Create(JsonSerializer.SerializeToElement(obj, options ?? JOptions.Default));
    }

    /// <summary>
    /// Creates a new instance from an existing <see cref="JsonValue"/>.
    /// </summary>
    public static JsonValue? Clone(this JsonValue? value) => value?.DeepClone().AsValue();
}
