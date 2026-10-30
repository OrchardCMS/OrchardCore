using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Telemetry;

internal static class TenantTelemetryServiceCollectionExtensions
{
    /// <summary>
    /// Registers the services tagging the HTTP requests with their tenant, once whatever the number of instrumented signals.
    /// </summary>
    public static IServiceCollection AddTenantTelemetry(this IServiceCollection services)
    {
        if (services.Any(descriptor => descriptor.ImplementationType == typeof(TenantTelemetryStartupFilter)))
        {
            return services;
        }

        services.AddSingleton<IStartupFilter, TenantTelemetryStartupFilter>();
        services.AddOptions<OrchardCoreOpenTelemetryOptions>().BindConfiguration(OrchardCoreOpenTelemetryOptions.ConfigurationSectionName);

        return services;
    }
}
