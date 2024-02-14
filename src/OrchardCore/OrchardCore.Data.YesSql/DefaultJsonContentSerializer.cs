using System;
using OrchardCore.Json;

namespace YesSql.Serialization;

public class DefaultJsonContentSerializer : IContentSerializer
{
    private readonly IJsonSerializer _jsonSerializer;

    public DefaultJsonContentSerializer(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public object Deserialize(string content, Type type)
        => _jsonSerializer.Deserialize(content, type);

    public string Serialize(object item)
        => _jsonSerializer.Serialize(item);
}
