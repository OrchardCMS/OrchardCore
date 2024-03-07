using System;
using System.Text.Json;

namespace OrchardCore.Json;

public class ContentSerializerJsonOptions
{
    public JsonSerializerOptions SerializerOptions { get; }

    public ContentSerializerJsonOptions()
    {
        SerializerOptions = new JsonSerializerOptions();
    }

    public ContentSerializerJsonOptions(JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        SerializerOptions = options;
    }
}
