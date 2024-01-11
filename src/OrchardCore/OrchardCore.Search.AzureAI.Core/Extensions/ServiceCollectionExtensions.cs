using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Recipes;
using OrchardCore.Search.AzureAI.Handlers;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Recipes;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.AzureAI;

public static class ServiceCollectionExtensions
{
    public static bool TryAddAzureAISearchServices(this IServiceCollection services, IShellConfiguration configuration, ILogger logger)
    {
        var section = configuration.GetSection("OrchardCore_AzureAISearch");

        var options = section.Get<AzureAISearchDefaultOptions>();
        var configExists = true;
        if (string.IsNullOrWhiteSpace(options?.Endpoint) || string.IsNullOrWhiteSpace(options?.Credential?.Key))
        {
            configExists = false;
            logger.LogError("Azure AI Search module is enabled. However, the connection settings are not provided in configuration file.");
        }

        services.AddTransient<IConfigureOptions<AzureAISearchDefaultOptions>, AzureAISearchDefaultOptionsConfigurations>();

        services.AddAzureClientsCore();
        services.AddSingleton<SearchIndexClientAccessor>();

        services.AddScoped<IPermissionProvider, Permissions>();
        services.AddScoped<IContentHandler, AzureAISearchIndexingContentHandler>();
        services.AddScoped<AzureAISearchIndexManager>();
        services.AddScoped<AzureAIIndexDocumentManager>();
        services.AddScoped<AzureAISearchIndexingService>();
        services.AddSingleton<AzureAISearchIndexSettingsService>();
        services.AddSingleton<SearchClientFactory>();

        services.AddRecipeExecutionStep<AzureAISearchIndexRebuildStep>();
        services.AddRecipeExecutionStep<AzureAISearchIndexResetStep>();
        services.AddRecipeExecutionStep<AzureAISearchIndexSettingsStep>();

        return configExists;
    }
}
