#nullable enable

namespace System.Text.Json.Nodes;

public static class JObject
{
    /// <summary>
    /// Creates a <see cref="JsonObject"/> from an object.
    /// </summary>
    public static JsonObject? FromObject(object? obj, JsonSerializerOptions? options = null) =>
        JsonObject.Create(JsonSerializer.SerializeToElement(obj, options));

    /// <summary>
    /// Creates a new instance from an existing <see cref="JsonObject"/>.
    /// </summary>
    public static JsonObject? Clone(this JsonObject? node) => node?.DeepClone().AsObject();

    /// <summary>
    /// Merge the specified content into this <see cref="JsonObject"/> using <see cref="JsonMergeSettings"/>.
    /// </summary>
    public static void Merge(this JsonObject? jsonObject, JsonObject? jsonContent, JsonMergeSettings? settings = null)
    {
        if (jsonObject is null || jsonContent is null)
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
                jsonObject.Add(item.Key, item.Value.Clone());
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
