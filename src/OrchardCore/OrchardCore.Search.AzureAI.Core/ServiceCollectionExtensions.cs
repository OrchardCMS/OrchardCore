using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing.Core;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;

namespace OrchardCore.Search.AzureAI.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureAISearchIndexingSource(this IServiceCollection services, string implementationType, Action<IndexingOptionsEntry> action = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(implementationType);

        services.AddIndexingSource<AzureAISearchIndexManager, AzureAISearchDocumentIndexManager, AzureAISearchIndexNameProvider, AzureAISearchDefaultOptions>(AzureAISearchConstants.ProviderName, implementationType, action);

        return services;
    }
}
