using Elastic.Clients.Elasticsearch;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public interface IElasticsearchClientFactory
{
    ElasticsearchClient Create(ElasticsearchConnectionOptions configuration);
}
