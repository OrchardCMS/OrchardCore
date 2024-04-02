using System.Text.Json;

namespace OrchardCore.Json;

public class DocumentJsonSerializerOptions
{
    public JsonSerializerOptions SerializerOptions { get; } = new JsonSerializerOptions();
}
