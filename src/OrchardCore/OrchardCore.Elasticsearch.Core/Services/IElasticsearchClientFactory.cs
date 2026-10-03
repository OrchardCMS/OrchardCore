using Elastic.Clients.Elasticsearch;
using OrchardCore.Elasticsearch.Core.Models;

namespace OrchardCore.Elasticsearch.Core.Services;

public interface IElasticsearchClientFactory
{
    ElasticsearchClient Create(ElasticsearchConnectionOptions configuration);
}