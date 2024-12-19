using System.Text.Json.Settings;
using Json.Path;

#nullable enable

namespace System.Text.Json.Nodes;

public static class JNode
{
    /// <summary>
    /// Loads a JSON node (including objects or arrays) from the provided stream.
    /// </summary>
    public static Task<JsonNode?> LoadAsync(Stream utf8Json) => LoadAsync(utf8Json, JOptions.Node, JOptions.Document);

    /// <summary>
    /// Loads a JSON node (including objects or arrays) from the provided stream.
    /// </summary>
    public static async Task<JsonNode?> LoadAsync(
        Stream utf8Json,
        JsonNodeOptions? nodeOptions = null,
        JsonDocumentOptions documentOptions = default,
        CancellationToken cancellationToken = default)
        => (await JsonNode.ParseAsync(utf8Json, nodeOptions ?? JOptions.Node, documentOptions, cancellationToken));

    /// <summary>
    /// Loads a JSON node (including objects or arrays) from the provided reader.
    /// </summary>
    public static JsonNode? Load(ref Utf8JsonReader reader, JsonNodeOptions? nodeOptions = null)
        => JsonNode.Parse(ref reader, nodeOptions ?? JOptions.Node);

    /// <summary>
    /// Parses text representing a single JSON node.
    /// </summary>
    public static JsonNode? Parse(string json) => Parse(json, JOptions.Node, JOptions.Document);

    /// <summary>
    /// Tries to parse text representing a single JSON node.
    /// </summary>
    public static bool TryParse(string json, out JsonNode? jsonNode) => TryParse(json, out jsonNode, JOptions.Node, JOptions.Document);

    /// <summary>
    /// Parses text representing a single JSON node.
    /// </summary>
    public static JsonNode? Parse(string json, JsonNodeOptions? nodeOptions = null, JsonDocumentOptions documentOptions = default)
        => JsonNode.Parse(json, nodeOptions ?? JOptions.Node, documentOptions);

    /// <summary>
    /// Tries to parse text representing a single JSON node.
    /// </summary>
    public static bool TryParse(string json, out JsonNode? jsonNode, JsonNodeOptions? nodeOptions = null, JsonDocumentOptions documentOptions = default)
    {
        try
        {
            jsonNode = JsonNode.Parse(json, nodeOptions ?? JOptions.Node, documentOptions);
            return true;
        }
        catch (JsonException)
        {
            jsonNode = null;
            return false;
        }
    }

    /// <summary>
    /// Creates a <see cref="JsonNode"/> from an object.
    /// </summary>
    public static JsonNode? FromObject(object? obj, JsonSerializerOptions? options = null)
    {
        if (obj is JsonNode jsonNode)
        {
            return jsonNode;
        }

        if (obj is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.Object => JsonObject.Create(jsonElement, JOptions.Node),
                JsonValueKind.Array => JsonArray.Create(jsonElement, JOptions.Node),
                _ => JsonValue.Create(jsonElement, JOptions.Node),
            };
        }

        return JsonSerializer.SerializeToNode(obj, options ?? JOptions.Default);
    }

    /// <summary>
    /// Creates a new instance from an existing <see cref="JsonNode"/>.
    /// </summary>
    public static JsonNode? Clone(this JsonNode? jsonNode) => jsonNode?.DeepClone();

    /// <summary>
    /// Creates an instance of the specified type from this <see cref="JsonNode"/>.
    /// </summary>
    public static T? ToObject<T>(this JsonNode? jsonNode, JsonSerializerOptions? options = null) =>
        jsonNode.Deserialize<T>(options ?? JOptions.Default);

    /// <summary>
    /// Creates an instance of the specified type from this <see cref="JsonNode"/>.
    /// </summary>
    public static object? ToObject(this JsonNode? jsonNode, Type type, JsonSerializerOptions? options = null) =>
        jsonNode.Deserialize(type, options ?? JOptions.Default);

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
    /// Gets the normalized JSON path by skipping the root part '$'.
    /// </summary>
    public static string? GetNormalizedPath(this string? path)
    {
        if (path is null || path.Length == 0 || path[0] != '$')
        {
            return path;
        }

        if (path.Length == 1)
        {
            return string.Empty;
        }

        return path[2..];
    }

    /// <summary>
    /// Gets the normalized JSON path by skipping the root path '$'.
    /// </summary>
    public static string? GetNormalizedPath(this JsonNode jsonNode) => jsonNode.GetPath().GetNormalizedPath();

    /// <summary>
    /// Selects a <see cref="JsonNode"/> from this <see cref="JsonObject"/> using JSONPath.
    /// </summary>
    /// <param name="jsonNode">The JSON node which serves as the root of the current search..</param>
    /// <param name="path">The JSONPath query where <c>$</c> is <paramref name="jsonNode"/>.</param>
    /// <param name="options">Optional settings to configure the JSONPath parser.</param>
    /// <remarks>
    /// This method uses JsonPath.Net to evaluate the <paramref name="path"/>. For more information on JSONPath, see the
    /// specification at https://www.rfc-editor.org/rfc/rfc9535.html or the JsonPath.Net documentation at
    /// https://docs.json-everything.net/path/basics/.
    /// </remarks>
    public static JsonNode? SelectNode(this JsonNode jsonNode, string path, PathParsingOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(jsonNode);
        ArgumentNullException.ThrowIfNull(path);

        path = path.Trim();
        if (string.IsNullOrEmpty(path))
        {
            return jsonNode;
        }

        // Without this JSONPath parsing fails (JsonPath.Parse throws "PathParseException: Path must start with '$'").
        // Since the path always has to start with "$.", this alteration won't cause any problems. It's also necessary
        // to maintain backwards compatibility with JSONPaths written for Newtonsoft and for simple direct child
        // property access too (e.g. when the path is "property" instead of "$.property").
        if (path[0] != '$')
        {
            path = "$." + path;
        }

        return JsonPath.TryParse(path, out var jsonPath, options)
            ? jsonPath.Evaluate(jsonNode).Matches?.FirstOrDefault()?.Value
            : null;
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
