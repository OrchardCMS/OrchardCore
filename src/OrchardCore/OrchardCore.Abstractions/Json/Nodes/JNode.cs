using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

#nullable enable

namespace System.Text.Json.Nodes;

public static class JNode
{
    public static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    static JNode()
    {
        Options.Converters.Add(new JsonDynamicConverter());
    }

    /// <summary>
    /// Creates a <see cref="JsonNode"/> from an object.
    /// </summary>
    public static JsonNode? FromObject(object? obj, JsonSerializerOptions? options = null) =>
        JsonSerializer.SerializeToNode(obj, options ?? Options);

    /// <summary>
    /// Creates a new instance from an existing <see cref="JsonNode"/>.
    /// </summary>
    public static JsonNode? Clone(this JsonNode? jsonNode) => jsonNode?.DeepClone();

    /// <summary>
    /// Creates an instance of the specified type from this <see cref="JsonNode"/>.
    /// </summary>
    public static T? ToObject<T>(this JsonNode? jsonNode, JsonSerializerOptions? options = null) =>
        jsonNode.Deserialize<T>(options ?? Options);

    /// <summary>
    /// Creates an instance of the specified type from this <see cref="JsonNode"/>.
    /// </summary>
    public static object? ToObject(this JsonNode? jsonNode, Type type, JsonSerializerOptions? options = null) =>
        jsonNode.Deserialize(type, options ?? Options);

    /// <summary>
    /// Gets the value of the specified type of this <see cref="JsonNode"/>.
    /// </summary>
    public static T? Value<T>(this JsonNode? jsonNode) =>
        jsonNode is JsonValue jsonValue && jsonValue.TryGetValue<T>(out var value) ? value : default;

    /// <summary>
    /// Gets the value of the specified type of this <see cref="JsonNode"/>.
    /// </summary>
    public static T? ValueOrDefault<T>(this JsonNode? jsonNode, T defaultValue) =>
        jsonNode is JsonValue jsonValue && jsonValue.TryGetValue<T>(out var value) ? value : defaultValue;

    /// <summary>
    /// Gets the value of the specified type from the specified property of this <see cref="JsonNode"/>.
    /// </summary>
    public static T? Value<T>(this JsonNode? jsonNode, string name) => jsonNode is not null ? jsonNode[name].Value<T>() : default;

    /// <summary>
    /// Gets the value of the specified type from the specified property of this <see cref="JsonNode"/>.
    /// </summary>
    public static T? ValueOrDefault<T>(this JsonNode? jsonNode, string name, T defaultValue) =>
        jsonNode is not null ? jsonNode[name].ValueOrDefault<T>(defaultValue) : defaultValue;

    /// <summary>
    /// Gets the value of the specified type from the specified index of this <see cref="JsonNode"/>.
    /// </summary>
    public static T? Value<T>(this JsonNode? jsonNode, int index) => jsonNode is JsonArray jsonArray ? jsonArray[index].Value<T>() : default;

    /// <summary>
    /// Whether this node contains elements or not.
    /// </summary>
    public static bool HasValues(this JsonNode? jsonNode) =>
        jsonNode is JsonObject jsonObject && jsonObject.Count > 0 ||
        jsonNode is JsonArray jsonArray && jsonArray.Count > 0;

    /// <summary>
    /// Gets the values of the specified type of this <see cref="JsonNode"/>.
    /// </summary>
    public static IEnumerable<T?> Values<T>(this JsonNode? jsonNode) =>
        jsonNode is JsonArray jsonArray ? jsonArray.AsEnumerable().Select(node => node.Value<T>()) : Enumerable.Empty<T?>();

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
