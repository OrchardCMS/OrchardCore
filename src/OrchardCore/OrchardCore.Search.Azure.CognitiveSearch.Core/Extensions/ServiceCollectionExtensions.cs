using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Search.Azure.CognitiveSearch.Handlers;
using OrchardCore.Search.Azure.CognitiveSearch.Models;
using OrchardCore.Search.Azure.CognitiveSearch.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Search.Azure.CognitiveSearch;

public static class ServiceCollectionExtensions
{
    public static void AddAzureCognitiveSearchServices(this IServiceCollection services, IShellConfiguration configuration)
    {
        var section = configuration.GetSection("OrchardCore_CognitiveSearch_Azure");

        services.Configure<AzureCognitiveSearchOptions>(section)
            .AddAzureClients(builder =>
        {
            builder.AddSearchIndexClient(section);
        });
        services.AddScoped<IPermissionProvider, Permissions>();
        services.AddScoped<IAuthorizationHandler, AzureCognitiveSearchAuthorizationHandler>();
        services.AddScoped<IContentHandler, CognitiveSearchIndexingContentHandler>();
        services.AddScoped<AzureCognitiveSearchIndexManager>();
        services.AddScoped<AzureCognitiveSearchDocumentManager>();
        services.AddSingleton<CognitiveSearchIndexSettingsService>();
        services.AddSingleton<SearchClientFactory>();
    }
}
