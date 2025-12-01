using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing.Core;
using OrchardCore.Queries;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Elastic services.
    /// </summary>
    public static IServiceCollection AddElasticsearchServices(this IServiceCollection services)
    {
        services.AddScoped<ElasticsearchQueryService>();

        services.AddQuerySource<ElasticsearchQuerySource>(ElasticsearchQuerySource.SourceName);

        return services;
    }

    public static IServiceCollection AddElasticsearchIndexingSource(this IServiceCollection services, string implementationType, Action<IndexingOptionsEntry> action = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(implementationType);

        services.AddIndexingSource<ElasticsearchIndexManager, ElasticsearchDocumentIndexManager, ElasticsearchIndexNameProvider, ElasticsearchConnectionOptions>(ElasticsearchConstants.ProviderName, implementationType, action);

        return services;
    }
}
