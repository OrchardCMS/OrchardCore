using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace OrchardCore.Azure.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureOptions(this IServiceCollection services)
    {
        services.TryAddSingleton<IConfigureNamedOptions<AzureOptions>, AzureOptionsConfigurations>();

        return services;
    }
}
