using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

        services.AddScoped<IAzureAISearchFieldIndexEvents, DefaultAzureAISearchFieldIndexEvents>();
        services.AddSingleton<AzureAIClientFactory>();

        return services;
    }
}
