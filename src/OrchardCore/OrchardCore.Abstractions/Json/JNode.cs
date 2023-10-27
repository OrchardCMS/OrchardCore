using System.Text.Json.Serialization;

#nullable enable

namespace System.Text.Json.Nodes;

public static class JNode
{
    private static readonly JsonSerializerOptions _options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Creates a <see cref="JsonNode"/> from an object.
    /// </summary>
    public static JsonNode? FromObject(object? obj, JsonSerializerOptions? options = null) =>
        JsonSerializer.SerializeToNode(obj, options);

    /// <summary>
    /// Creates a new instance from an existing <see cref="JsonNode"/>.
    /// </summary>
    public static JsonNode? Clone(this JsonNode? jsonNode) => jsonNode?.DeepClone();

    /// <summary>
    /// Creates an instance of the specified type from this <see cref="JsonNode"/>.
    /// </summary>
    public static T? ToObject<T>(this JsonNode? jsonNode, JsonSerializerOptions? options = null) =>
        jsonNode.Deserialize<T>(options ?? _options);

    /// <summary>
    /// Creates an instance of the specified type from this <see cref="JsonNode"/>.
    /// </summary>
    public static object? ToObject(this JsonNode? jsonNode, Type type, JsonSerializerOptions? options = null) =>
        jsonNode.Deserialize(type, options ?? _options);

    /// <summary>
    /// Gets the value of the specified type of this <see cref="JsonNode"/>.
    /// </summary>
    public static T? Value<T>(this JsonNode? jsonNode) =>
        jsonNode is JsonValue jsonValue && jsonValue.TryGetValue<T>(out var value) ? value : default;

    /// <summary>
    /// Gets the value of the specified type from the specified property of this <see cref="JsonNode"/>.
    /// </summary>
    public static T? Value<T>(this JsonNode? jsonNode, string name) => jsonNode is not null ? jsonNode[name].Value<T>() : default;

    /// <summary>
    /// Gets the value of the specified type from the specified index of this <see cref="JsonNode"/>.
    /// </summary>
    public static T? Value<T>(this JsonNode? node, int index) => node is not null ? node[index].Value<T>() : default;

    /// <summary>
    /// Selects a <see cref="JsonNode"/> from this <see cref="JsonObject"/> using a JSON path.
    /// </summary>
    public static JsonNode? SelectNode(this JsonNode? jsonNode, string? path)
    {
        if (jsonNode is null || path is null)
        {
            return null;
        }

        if (jsonNode is JsonObject jsonObject)
        {
            return jsonObject.SelectNode(path);
        }

        if (jsonNode is JsonArray jsonArray)
        {
            return jsonArray.SelectNode(path);
        }

        return null;
    }

    /// <summary>
    /// Merge the specified content into this <see cref="JsonNode"/> using <see cref="JsonMergeSettings"/>.
    /// </summary>
    internal static void Merge(this JsonNode? jsonNode, JsonNode? content, JsonMergeSettings? settings = null)
    {
        settings ??= new JsonMergeSettings();

        if (jsonNode is JsonObject jsonObject)
        {
            jsonObject.Merge(content, settings);

            return;
        }

        if (jsonNode is JsonArray jsonArray)
        {
            jsonArray.Merge(content, settings);
        }
    }
}
