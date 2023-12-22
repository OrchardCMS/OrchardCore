using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Search.AzureAI.Handlers;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.AzureAI;

public static class ServiceCollectionExtensions
{
    public static bool TryAddAzureAISearchServices(this IServiceCollection services, IShellConfiguration configuration, ILogger logger)
    {
        var section = configuration.GetSection("OrchardCore_AzureAISearch");

        var options = section.Get<AzureAIOptions>();

        if (string.IsNullOrWhiteSpace(options.Endpoint) || string.IsNullOrWhiteSpace(options.Credential?.Key))
        {
            logger.LogError("Azure AI Search module is enabled. However, the connection settings are not provided.");

            return false;
        }

        services.Configure<AzureAIOptions>(section);
        services.AddAzureClients(builder =>
        {
            builder.AddSearchIndexClient(section);
        });
        services.AddScoped<IPermissionProvider, Permissions>();
        services.AddScoped<IAuthorizationHandler, AzureAIAuthorizationHandler>();
        services.AddScoped<IContentHandler, AzureAIIndexingContentHandler>();
        services.AddScoped<AzureAIIndexManager>();
        services.AddScoped<AzureAIIndexDocumentManager>();
        services.AddSingleton<AzureAIIndexSettingsService>();
        services.AddSingleton<SearchClientFactory>();

        return true;
    }
}
