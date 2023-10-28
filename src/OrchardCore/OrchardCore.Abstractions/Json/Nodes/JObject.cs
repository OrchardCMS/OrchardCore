#nullable enable

using Microsoft.Extensions.Options;

namespace System.Text.Json.Nodes;

public static class JObject
{
    /// <summary>
    /// Creates a <see cref="JsonObject"/> from an object.
    /// </summary>
    public static JsonObject? FromObject(object? obj, JsonSerializerOptions? options = null) =>
        JsonObject.Create(JsonSerializer.SerializeToElement(obj, options ?? JNode.Options));

    /// <summary>
    /// Creates a new instance from an existing <see cref="JsonObject"/>.
    /// </summary>
    public static JsonObject? Clone(this JsonObject? jsonObject) => jsonObject?.DeepClone().AsObject();

    /// <summary>
    /// Selects a <see cref="JsonNode"/> from this <see cref="JsonObject"/> using a JSON path.
    /// </summary>
    public static JsonNode? SelectNode(this JsonObject? jsonObject, string? path)
    {
        if (jsonObject is null || path is null)
        {
            return null;
        }

        foreach (var item in jsonObject)
        {
            if (item.Value is null)
            {
                continue;
            }

            var itemPath = item.Value.GetPath();
            if (itemPath == path)
            {
                return item.Value;
            }

            if (!path.Contains(itemPath))
            {
                continue;
            }

            if (item.Value is JsonObject jObject)
            {
                var node = jObject.SelectNode(path);
                if (node is not null)
                {
                    return node;
                }
            }

            if (item.Value is JsonArray jArray)
            {
                var node = jArray.SelectNode(path);
                if (node is not null)
                {
                    return node;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Merge the specified content into this <see cref="JsonObject"/> using <see cref="JsonMergeSettings"/>.
    /// </summary>
    public static void Merge(this JsonObject? jsonObject, JsonNode? content, JsonMergeSettings? settings = null)
    {
        if (jsonObject is null || content is not JsonObject jsonContent)
        {
            return;
        }

        settings ??= new JsonMergeSettings();

        foreach (var item in jsonContent)
        {
            if (item.Value is null)
            {
                continue;
            }

            var existingProperty = jsonObject[item.Key];
            if (existingProperty is null)
            {
                jsonObject[item.Key] = item.Value.Clone();
                continue;
            }

            if (existingProperty is JsonObject jObject)
            {
                jObject.Merge(item.Value, settings);
                continue;
            }

            if (existingProperty is JsonArray jArray)
            {
                jArray.Merge(item.Value, settings);
                continue;
            }

            if (existingProperty is JsonValue || existingProperty.GetValueKind() != item.Value.GetValueKind())
            {
                if (item.Value.GetValueKind() != JsonValueKind.Null ||
                    settings?.MergeNullValueHandling == MergeNullValueHandling.Merge)
                {
                    jsonObject[item.Key] = item.Value.Clone();
                }
            }
        }
    }
}
