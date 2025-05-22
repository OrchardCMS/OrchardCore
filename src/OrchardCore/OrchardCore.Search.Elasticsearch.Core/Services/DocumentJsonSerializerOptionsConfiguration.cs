using Elastic.Clients.Elasticsearch;
using Elastic.Transport.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Json;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

internal sealed class DocumentJsonSerializerOptionsConfiguration : IConfigureOptions<DocumentJsonSerializerOptions>
{
    private readonly ElasticsearchClient _elasticsearchClient;

    public DocumentJsonSerializerOptionsConfiguration(ElasticsearchClient elasticsearchClient)
    {
        _elasticsearchClient = elasticsearchClient;
    }

    public void Configure(DocumentJsonSerializerOptions options)
    {
        if (!_elasticsearchClient.RequestResponseSerializer.TryGetJsonSerializerOptions(out var sourceSerializerOptions))
        {
            return;
        }

        foreach (var converter in sourceSerializerOptions.Converters)
        {
            options.SerializerOptions.Converters.Add(converter);
        }
    }
}
