using System.Text.Json.Settings;

namespace System.Text.Json.Nodes;

#nullable enable

public static class JArray
{
    /// <summary>
    /// Loads a JSON array from the provided stream.
    /// </summary>
    public static async Task<JsonArray?> LoadAsync(Stream utf8Json) => (await JNode.LoadAsync(utf8Json))?.AsArray();

    /// <summary>
    /// Loads a JSON array from the provided stream.
    /// </summary>
    public static async Task<JsonArray?> LoadAsync(
        Stream utf8Json,
        JsonNodeOptions? nodeOptions = null,
        JsonDocumentOptions documentOptions = default,
        CancellationToken cancellationToken = default)
        => (await JNode.LoadAsync(utf8Json, nodeOptions, documentOptions, cancellationToken))?.AsArray();

    /// <summary>
    /// Loads a JSON array from the provided reader.
    /// </summary>
    public static JsonArray? Load(ref Utf8JsonReader reader, JsonNodeOptions? nodeOptions = null)
        => JNode.Load(ref reader, nodeOptions)?.AsArray();

    /// <summary>
    /// Parses text representing a single JSON array.
    /// </summary>
    public static JsonArray? Parse(string json) => JNode.Parse(json)?.AsArray();

    /// <summary>
    /// Tries to parse text representing a single JSON array.
    /// </summary>
    public static bool TryParse(string json, out JsonArray? jsonArray) => TryParse(json, out jsonArray, JOptions.Node, JOptions.Document);

    /// <summary>
    /// Parses text representing a single JSON array.
    /// </summary>
    public static JsonArray? Parse(string json, JsonNodeOptions? nodeOptions = null, JsonDocumentOptions documentOptions = default)
        => JNode.Parse(json, nodeOptions, documentOptions)?.AsArray();

    /// <summary>
    /// Tries to parse text representing a single JSON array.
    /// </summary>
    public static bool TryParse(string json, out JsonArray? jsonArray, JsonNodeOptions? nodeOptions = null, JsonDocumentOptions documentOptions = default)
    {
        if (!JNode.TryParse(json, out var jsonNode, nodeOptions, documentOptions) ||
            jsonNode is not JsonArray jArray)
        {
            jsonArray = null;
            return false;
        }

        jsonArray = jArray;
        return true;
    }

    /// <summary>
    /// Creates a <see cref="JsonArray"/> from an object.
    /// </summary>
    public static JsonArray? FromObject(object? obj, JsonSerializerOptions? options = null)
    {
        if (obj is JsonElement jsonElement)
        {
            return JsonArray.Create(jsonElement);
        }

        return JsonArray.Create(JsonSerializer.SerializeToElement(obj, options ?? JOptions.Default));
    }

    /// <summary>
    /// Creates a new instance from an existing <see cref="JsonArray"/>.
    /// </summary>
    public static JsonArray? Clone(this JsonArray? jsonArray) => jsonArray?.DeepClone().AsArray();

    /// <summary>
    /// Whether this <see cref="JsonArray"/> contains the provided <see cref="JsonValue"/> or not.
    /// </summary>
    public static bool ContainsValue(this JsonArray? jsonArray, JsonValue? value)
    {
        if (jsonArray is null || value is null)
        {
            return false;
        }

        foreach (var item in jsonArray)
        {
            if (item is not JsonValue)
            {
                continue;
            }

            if (JsonNode.DeepEquals(item, value))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Merge the specified content into this <see cref="JsonArray"/> using <see cref="JsonMergeSettings"/>.
    /// </summary>
    internal static JsonArray? Merge(this JsonArray? jsonArray, JsonNode? content, JsonMergeSettings? settings = null)
    {
        if (jsonArray is null || content is not JsonArray jsonContent)
        {
            return jsonArray;
        }

        settings ??= new JsonMergeSettings();

        switch (settings.MergeArrayHandling)
        {
            case MergeArrayHandling.Concat:

                foreach (var item in jsonContent)
                {
                    jsonArray.Add(item.Clone());
                }

                break;

            // 'Union' only for arrays of 'JsonValue's, otherwise acts as `Concat`,
            // this to prevent many expensive 'DeepEquals()' on more complex items.
            case MergeArrayHandling.Union:

                foreach (var item in jsonContent)
                {
                    // Only checking for existing 'JsonValue'.
                    if (item is JsonValue jsonValue && jsonArray.ContainsValue(jsonValue))
                    {
                        continue;
                    }

                    jsonArray.Add(item.Clone());
                }

                break;

            case MergeArrayHandling.Replace:

                if (jsonArray == jsonContent)
                {
                    break;
                }

                jsonArray.Clear();
                foreach (var item in jsonContent)
                {
                    jsonArray.Add(item.Clone());
                }

                break;

            case MergeArrayHandling.Merge:

                for (var i = 0; i < jsonContent.Count; i++)
                {
                    var item = jsonContent[i];
                    if (item is null)
                    {
                        continue;
                    }

                    if (i < jsonArray.Count)
                    {
                        var existingItem = jsonArray[i];
                        if (existingItem is null)
                        {
                            jsonArray[i] = item.Clone();
                            continue;
                        }

                        if (existingItem is JsonObject jObject)
                        {
                            jObject.Merge(item, settings);
                            continue;
                        }

                        if (existingItem is JsonArray jArray)
                        {
                            jArray.Merge(item, settings);
                            continue;
                        }

                        if (existingItem is JsonValue || existingItem.GetValueKind() != item.GetValueKind())
                        {
                            if (item.GetValueKind() != JsonValueKind.Null ||
                                settings?.MergeNullValueHandling == MergeNullValueHandling.Merge)
                            {
                                jsonArray[i] = item.Clone();
                            }
                        }
                    }
                    else
                    {
                        jsonArray.Add(item.Clone());
                    }
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(settings), "Unexpected merge array handling when merging JSON.");
        }

        return jsonArray;
    }
}
