using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing.Core;
using OrchardCore.AzureAI.Services;

namespace OrchardCore.AzureAI.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureAISearchIndexingSource(this IServiceCollection services, string implementationType, Action<IndexingOptionsEntry> action = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(implementationType);

        services.AddIndexingSource<AzureAISearchIndexManager, AzureAISearchDocumentIndexManager, AzureAISearchIndexNameProvider>(AzureAISearchConstants.ProviderName, implementationType, action);

        return services;
    }
}
