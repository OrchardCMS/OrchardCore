using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.AzureAI.Handlers;
using OrchardCore.AzureAI.Models;
using OrchardCore.AzureAI.Services;

namespace OrchardCore.AzureAI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureAISearchServices(this IServiceCollection services)
    {
        services.AddAzureClientsCore();
        services.AddTransient<IConfigureOptions<AzureAISearchDefaultOptions>, AzureAISearchDefaultOptionsConfigurations>();
        services.AddScoped<AzureAISearchContentFieldMapper>();
        services.AddScoped<IAzureAISearchFieldIndexEvents, DefaultAzureAISearchFieldIndexEvents>();
        services.AddSingleton<AzureAIClientFactory>();

        return services;
    }
}
