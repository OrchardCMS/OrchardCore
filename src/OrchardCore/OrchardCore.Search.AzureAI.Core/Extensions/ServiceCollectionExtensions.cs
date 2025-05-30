using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Search.AzureAI.Handlers;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.AzureAI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureAISearchServices(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<AzureAISearchDefaultOptions>, AzureAISearchDefaultOptionsConfigurations>();
        services.AddAzureClientsCore();
        services.AddPermissionProvider<Permissions>();
        services.AddScoped<IContentHandler, AzureAISearchIndexingContentHandler>();
        services.AddScoped<AzureAISearchIndexManager>();
        services.AddScoped<AzureAIIndexDocumentManager>();
        services.AddScoped<AzureAISearchIndexingService>();
        services.AddScoped<IAzureAISearchFieldIndexEvents, DefaultAzureAISearchFieldIndexEvents>();
        services.AddScoped<AzureAISearchIndexSettingsService>();
        services.AddSingleton<AzureAIClientFactory>();
        services.AddSingleton<AzureAISearchIndexNameService>();

        return services;
    }
}
