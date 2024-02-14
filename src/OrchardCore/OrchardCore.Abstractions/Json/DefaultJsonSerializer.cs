using System;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace OrchardCore.Json;

public class DefaultJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerOptions _options;

    public DefaultJsonSerializer(IOptions<JsonSerializerOptions> options)
    {
        _options = options.Value;
    }

    public object Deserialize(string content, Type type)
        => JsonSerializer.Deserialize(content, type, _options);

    public T Deserialize<T>(string content)
        => JsonSerializer.Deserialize<T>(content, _options);

    public string Serialize(object item)
        => JsonSerializer.Serialize(item, _options);

    public string Serialize<T>(T item)
        => JsonSerializer.Serialize(item, _options);
}
