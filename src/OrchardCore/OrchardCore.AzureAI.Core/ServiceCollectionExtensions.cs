using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.AzureAI.Services;
using OrchardCore.Indexing.Core;

namespace OrchardCore.AzureAI.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureAISearchIndexingSource(
        this IServiceCollection services,
        string implementationType,
        Action<IndexingOptionsEntry> action = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(implementationType);

        services.AddIndexingSource<AzureAISearchIndexManager, AzureAISearchDocumentIndexManager, AzureAISearchIndexNameProvider>(
            AzureAISearchConstants.ProviderName, implementationType, action);

        services
            .AddOptions<IndexingOptions>()
            .Configure<IStringLocalizer<AzureAISearchLocalizationMarker>>((options, S) =>
                options.AddIndexingProvider(AzureAISearchConstants.ProviderName, provider => provider.DisplayName = S["Azure AI Search"]));

        return services;
    }

    private sealed class AzureAISearchLocalizationMarker
    {
    }
}
