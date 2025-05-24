using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Indexing;
using OrchardCore.Search.AzureAI.Handlers;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;

namespace OrchardCore.Search.AzureAI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureAISearchServices(this IServiceCollection services)
    {
        services.AddAzureClientsCore();
        services.AddTransient<IConfigureOptions<AzureAISearchDefaultOptions>, AzureAISearchDefaultOptionsConfigurations>();
        services.AddKeyedScoped<IIndexManager, AzureAISearchIndexManager>(AzureAISearchConstants.ProviderName);
        services.AddScoped<AzureAIIndexDocumentManager>();
        services.AddKeyedScoped<IIndexDocumentManager>(AzureAISearchConstants.ProviderName, (sp, key) => sp.GetService<AzureAIIndexDocumentManager>());

        services.AddScoped<IAzureAISearchFieldIndexEvents, DefaultAzureAISearchFieldIndexEvents>();
        services.AddSingleton<AzureAIClientFactory>();
        services.AddSingleton<AzureAISearchIndexNameService>();

        return services;
    }
}
