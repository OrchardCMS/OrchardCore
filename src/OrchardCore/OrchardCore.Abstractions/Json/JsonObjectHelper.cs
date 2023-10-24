#nullable enable

namespace System.Text.Json.Nodes;

public static class JObject
{
    /// <summary>
    /// Creates a <see cref="JsonObject"/> from an object.
    /// </summary>
    public static JsonObject? FromObject(object? obj, JsonSerializerOptions? options = null) =>
        JsonObject.Create(JsonSerializer.SerializeToElement(obj, options));
}

public static class JArray
{
    /// <summary>
    /// Creates a <see cref="JsonArray"/> from an object.
    /// </summary>
    public static JsonArray? FromObject(object? obj, JsonSerializerOptions? options = null) =>
        JsonArray.Create(JsonSerializer.SerializeToElement(obj, options));
}

public static class JNode
{
    /// <summary>
    /// Creates a <see cref="JsonNode"/> from an object.
    /// </summary>
    public static JsonNode? FromObject(object? obj, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.SerializeToNode(obj, options);
    }
}

public static class JsonNodeExtensions
{
    /// <summary>
    /// Creates an instance of the specified type from this <see cref="JsonNode"/>.
    /// </summary>
    public static T? ToObject<T>(this JsonNode? node) => node.Deserialize<T>();

    /// <summary>
    /// Creates an instance of the specified type from this <see cref="JsonNode"/>.
    /// </summary>
    public static object? ToObject(this JsonNode? node, Type type) => node.Deserialize(type);

    /// <summary>
    /// Gets the value of the specified type of this <see cref="JsonNode"/>.
    /// </summary>
    public static T? Value<T>(this JsonNode? node)
    {
        if (node is JsonValue jsonValue && jsonValue.TryGetValue<T>(out var value))
        {
            return value;
        }

        return default;
    }

    /// <summary>
    /// Gets the value of the specified type from the specified property of this <see cref="JsonNode"/>.
    /// </summary>
    public static T? Value<T>(this JsonNode? node, string name)
    {
        if (node is JsonObject jsonObject && jsonObject.TryGetPropertyValue(name, out var property))
        {
            return property.Value<T>();
        }

        return default;
    }
}
