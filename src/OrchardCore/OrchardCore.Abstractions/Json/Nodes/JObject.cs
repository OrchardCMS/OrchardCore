using System.Text.Json.Settings;

namespace System.Text.Json.Nodes;

#nullable enable

public static class JObject
{
    /// <summary>
    /// Loads a JSON object from the provided stream.
    /// </summary>
    public static async Task<JsonObject?> LoadAsync(Stream utf8Json) => (await JNode.LoadAsync(utf8Json))?.AsObject();

    /// <summary>
    /// Loads a JSON object from the provided stream.
    /// </summary>
    public static async Task<JsonObject?> LoadAsync(
        Stream utf8Json,
        JsonNodeOptions? nodeOptions = null,
        JsonDocumentOptions documentOptions = default,
        CancellationToken cancellationToken = default)
        => (await JNode.LoadAsync(utf8Json, nodeOptions, documentOptions, cancellationToken))?.AsObject();

    /// <summary>
    /// Loads a JSON object from the provided reader.
    /// </summary>
    public static JsonObject? Load(ref Utf8JsonReader reader, JsonNodeOptions? nodeOptions = null)
        => JNode.Load(ref reader, nodeOptions)?.AsObject();

    /// <summary>
    /// Parses text representing a single JSON object.
    /// </summary>
    public static JsonObject? Parse(string json) => JNode.Parse(json)?.AsObject();

    /// <summary>
    /// Tries to parse text representing a single JSON object.
    /// </summary>
    public static bool TryParse(string json, out JsonObject? jsonObject) => TryParse(json, out jsonObject, JOptions.Node, JOptions.Document);

    /// <summary>
    /// Parses text representing a single JSON object.
    /// </summary>
    public static JsonObject? Parse(string json, JsonNodeOptions? nodeOptions = null, JsonDocumentOptions documentOptions = default)
        => JNode.Parse(json, nodeOptions, documentOptions)?.AsObject();

    /// <summary>
    /// Tries to parse text representing a single JSON object.
    /// </summary>
    public static bool TryParse(string json, out JsonObject? jsonObject, JsonNodeOptions? nodeOptions = null, JsonDocumentOptions documentOptions = default)
    {
        if (!JNode.TryParse(json, out var jsonNode, nodeOptions, documentOptions) ||
            jsonNode is not JsonObject jObject)
        {
            jsonObject = null;
            return false;
        }

        jsonObject = jObject;
        return true;
    }

    /// <summary>
    /// Creates a <see cref="JsonObject"/> from an object.
    /// </summary>
    public static JsonObject? FromObject(object? obj, JsonSerializerOptions? options = null)
    {
        if (obj is JsonObject jsonObject)
        {
            return jsonObject;
        }

        if (obj is JsonElement jsonElement)
        {
            return JsonObject.Create(jsonElement, JOptions.Node);
        }

        return JsonObject.Create(JsonSerializer.SerializeToElement(obj, options ?? JOptions.Default), JOptions.Node);
    }

    /// <summary>
    /// Creates a new instance from an existing <see cref="JsonObject"/>.
    /// </summary>
    public static JsonObject? Clone(this JsonObject? jsonObject) => jsonObject?.DeepClone().AsObject();

    /// <summary>
    /// Merge the specified content into this <see cref="JsonObject"/> using <see cref="JsonMergeSettings"/>.
    /// </summary>
    public static JsonObject? Merge(this JsonObject? jsonObject, JsonNode? content, JsonMergeSettings? settings = null)
    {
        if (jsonObject is null || content is not JsonObject jsonContent)
        {
            return jsonObject;
        }

        settings ??= new JsonMergeSettings();

        foreach (var item in jsonContent)
        {
            if (item.Value is null)
            {
                if (settings!.MergeNullValueHandling == MergeNullValueHandling.Merge)
                {
                    jsonObject[item.Key] = null;
                }

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
                    settings!.MergeNullValueHandling == MergeNullValueHandling.Merge)
                {
                    jsonObject[item.Key] = item.Value.Clone();
                }
            }
        }

        return jsonObject;
    }
}
