#nullable enable

using System.Linq;

namespace System.Text.Json.Nodes;

public static class JArray
{
    /// <summary>
    /// Creates a <see cref="JsonArray"/> from an object.
    /// </summary>
    public static JsonArray? FromObject(object? obj, JsonSerializerOptions? options = null) =>
        JsonArray.Create(JsonSerializer.SerializeToElement(obj, options));

    /// <summary>
    /// Creates a new instance from an existing <see cref="JsonArray"/>.
    /// </summary>
    public static JsonArray? Clone(this JsonArray? array) => array?.DeepClone().AsArray();

    /// <summary>
    /// Whether this <see cref="JsonArray"/> contains the provided <see cref="JsonArray"/> or not.
    /// </summary>
    public static bool ContainsValue(this JsonArray? array, JsonValue? value)
    {
        if (array is null || value is null)
        {
            return false;
        }

        foreach (var item in array)
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
    internal static void Merge(this JsonArray? jsonArray, JsonArray? jsonContent, JsonMergeSettings? settings = null)
    {
        if (jsonArray is null || jsonContent is null)
        {
            return;
        }

        settings ??= new JsonMergeSettings();

        switch (settings?.MergeArrayHandling ?? MergeArrayHandling.Concat)
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
    }
}
