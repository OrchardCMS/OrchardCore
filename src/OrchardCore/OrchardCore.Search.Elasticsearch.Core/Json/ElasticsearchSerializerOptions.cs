using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace OrchardCore.Search.Elasticsearch.Core.Json;

internal static class ElasticsearchSerializerOptions
{
    internal readonly static Serializer ElasticsearchRequestResponseSerializer;

    static ElasticsearchSerializerOptions()
    {
        var settings = new ElasticsearchClientSettings();

        ElasticsearchRequestResponseSerializer = new ElasticsearchClient(settings).RequestResponseSerializer;
    }
}
