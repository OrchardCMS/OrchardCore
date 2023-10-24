#nullable enable

namespace System.Text.Json.Nodes;

public static class JsonObjectExtensions
{

    /// <summary>
    /// Merge the specified content into this <see cref="JsonObject"/> using <see cref="JsonSerializerOptions"/>.
    /// </summary>
    public static void Merge(this JsonObject jsonObject, JsonObject? content, JsonSerializerOptions? options = null)
    {
        if (content is null)
        {
            return;
        }

        foreach (var item in content)
        {
            if (!jsonObject.TryGetPropertyValue(item.Key, out var existing))
            {
                jsonObject[item.Key] = item.Value;

                continue;
            }

            // Maybe a merge option options?.MergeNullValueHandling.Merge or Replace or MergeArrayHandling.Merge or Replace
            if (item.Value is null)
            {
                continue;
            }

            switch (item.Value.GetValueKind())
            {
                case JsonValueKind.Object:
                    // Merge Object.
                    // VisitObjectElement(value);
                    break;

                case JsonValueKind.Array:
                    // Merge Array.
                    // VisitArrayElement(value);
                    break;

                case JsonValueKind.Number:
                case JsonValueKind.String:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:

                    // Skipping null values is useful to override array items,
                    // it allows to keep non null items at the right position.
                    //if (visitArray && value.ValueKind == JsonValueKind.Null)
                    //{
                    //    break;
                    //}

                    jsonObject.Add(item.Key, item.Value);
                    break;
            }

        }
    }

    /// <summary>
    /// Merge the specified content into this <see cref="JsonObject"/> using <see cref="JsonSerializerOptions"/>.
    /// </summary>
    public static void Merge2(this JsonObject jsonObject, JsonObject? content, JsonSerializerOptions? options = null)
    {
        if (content is null)
        {
            return;
        }

        var jsonElement = JsonSerializer.Deserialize<JsonElement>(content, options);

        jsonObject.MergeItem(jsonElement, options);
    }

    public static void MergeItem(this JsonObject jsonObject, JsonElement content, JsonSerializerOptions? options)
    {
        foreach (var property in content.EnumerateObject())
        {
            if (jsonObject.ContainsKey(property.Name))
            {
                continue;
            }

            switch (property.Value.ValueKind)
            {
                case JsonValueKind.Object:
                    // Merge Object.
                    break;

                case JsonValueKind.Array:
                    // Skipping null values is useful to override array items,
                    // it allows to keep non null items at the right position.
                    // if (visitArray && value.ValueKind == JsonValueKind.Null)
                    // {
                    //     break;
                    // }

                    // Merge Array.
                    break;

                case JsonValueKind.Number:
                    if (property.Value.TryGetInt32(out var int32))
                    {
                        jsonObject.Add(property.Name, int32);
                    }
                    else if (property.Value.TryGetInt64(out var int64))
                    {
                        jsonObject.Add(property.Name, int64);
                    }
                    else if (property.Value.TryGetDouble(out var d))
                    {
                        jsonObject.Add(property.Name, d);
                    }
                    else
                    {
                        throw new JsonException($"Failed to parse number {property.Value}.");
                    }

                    break;

                case JsonValueKind.String:
                    jsonObject.Add(property.Name, property.Value.GetString());
                    break;


                case JsonValueKind.True:
                    jsonObject.Add(property.Name, true);
                    break;

                case JsonValueKind.False:
                    jsonObject.Add(property.Name, false);
                    break;

                case JsonValueKind.Null:
                    break;

                default:
                    throw new FormatException($"Unsupported JSON token '{property.Value.ValueKind}' was found.");
            }
        }
    }
}
