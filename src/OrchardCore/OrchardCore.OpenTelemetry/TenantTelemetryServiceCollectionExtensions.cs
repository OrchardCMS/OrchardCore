using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

        // The options of the telemetry emitted by Orchard Core itself, read by the tenants.
        services
            .AddOptions<OrchardCoreTelemetryOptions>()
            .Configure<IOptions<OrchardCoreOpenTelemetryOptions>>((options, openTelemetryOptions) =>
            {
                options.IncludeDatabaseQueryText = openTelemetryOptions.Value.Tracing.IncludeDatabaseQueryText;
            });

        return services;
    }
}
