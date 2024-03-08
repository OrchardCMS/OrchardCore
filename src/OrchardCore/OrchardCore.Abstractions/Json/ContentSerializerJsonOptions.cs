using System.Text.Json;

namespace OrchardCore.Json;

public class ContentSerializerJsonOptions
{
    public JsonSerializerOptions SerializerOptions { get; } = new JsonSerializerOptions();
}
