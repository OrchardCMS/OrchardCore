using System.Text.Json;
using Microsoft.Extensions.Options;
using OrchardCore.Json;

namespace YesSql.Serialization;

public class DefaultContentJsonSerializer : IContentSerializer
{
    private readonly JsonSerializerOptions _options;

    public DefaultContentJsonSerializer(IOptions<DocumentJsonSerializerOptions> options)
    {
        _options = options.Value.SerializerOptions;
    }

    public DefaultContentJsonSerializer(JsonSerializerOptions options)
        => _options = options;

    public object Deserialize(string content, Type type)
        => JsonSerializer.Deserialize(content, type, _options);

    public dynamic DeserializeDynamic(string content)
        => JsonSerializer.Deserialize<dynamic>(content, _options);

    public string Serialize(object item)
        => JsonSerializer.Serialize(item, _options);
}
