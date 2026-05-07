using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Elasticsearch.Core.Services;
using OrchardCore.Indexing.Core;
using OrchardCore.Queries;

namespace OrchardCore.Elasticsearch;

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

    public static IServiceCollection AddElasticsearchIndexingSource(
        this IServiceCollection services,
        string implementationType,
        Action<IndexingOptionsEntry> action = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(implementationType);

        services.AddIndexingSource<ElasticsearchIndexManager, ElasticsearchDocumentIndexManager, ElasticsearchIndexNameProvider>(
            ElasticsearchConstants.ProviderName, implementationType, action);

        services
            .AddOptions<IndexingOptions>()
            .Configure<IStringLocalizer<ElasticsearchLocalizationMarker>>((options, S) =>
                options.AddIndexingProvider(ElasticsearchConstants.ProviderName, provider => provider.DisplayName = S["Elasticsearch"]));

        return services;
    }

    private sealed class ElasticsearchLocalizationMarker
    {
    }
}
