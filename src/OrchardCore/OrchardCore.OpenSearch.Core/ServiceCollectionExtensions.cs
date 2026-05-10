using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing.Core;
using OrchardCore.Queries;
using OrchardCore.OpenSearch.Core.Models;
using OrchardCore.OpenSearch.Core.Services;

namespace OrchardCore.OpenSearch;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenSearch services.
    /// </summary>
    public static IServiceCollection AddOpenSearchServices(this IServiceCollection services)
    {
        services.AddScoped<OpenSearchQueryService>();

        services.AddQuerySource<OpenSearchQuerySource>(OpenSearchQuerySource.SourceName);

        return services;
    }

    public static IServiceCollection AddOpenSearchIndexingSource(this IServiceCollection services, string implementationType, Action<IndexingOptionsEntry> action = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(implementationType);

        services.AddIndexingSource<OpenSearchIndexManager, OpenSearchDocumentIndexManager, OpenSearchIndexNameProvider, OpenSearchConnectionOptions>(OpenSearchConstants.ProviderName, implementationType, action);

        return services;
    }
}
