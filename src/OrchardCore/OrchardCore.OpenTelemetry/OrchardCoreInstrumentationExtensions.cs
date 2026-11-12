using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OrchardCore.Environment.Shell;
using OrchardCore.Telemetry;

namespace OpenTelemetry;

/// <summary>
/// Extensions adding the Orchard Core instrumentation to an OpenTelemetry pipeline configured by the application.
/// </summary>
public static class OrchardCoreInstrumentationExtensions
{
    /// <summary>
    /// Collects the traces emitted by Orchard Core, and tags the spans with the name of their tenant.
    /// </summary>
    /// <param name="builder">The <see cref="TracerProviderBuilder"/> to configure.</param>
    /// <remarks>Call it before adding exporters, so that the spans are tagged before being exported.</remarks>
    public static TracerProviderBuilder AddOrchardCoreInstrumentation(this TracerProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .ConfigureServices(services => services.AddTenantTelemetry())
            .AddSource(OrchardCoreTelemetryConstants.ActivitySourceName)
            .AddProcessor(new TenantActivityProcessor());
    }

    /// <summary>
    /// Collects the metrics emitted by Orchard Core, including the number of tenants of this instance of the application,
    /// and tags the HTTP server metrics with the name of the tenant that served the request.
    /// </summary>
    /// <param name="builder">The <see cref="MeterProviderBuilder"/> to configure.</param>
    public static MeterProviderBuilder AddOrchardCoreInstrumentation(this MeterProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .ConfigureServices(services => services.AddMetrics().AddTenantTelemetry())
            .AddMeter(OrchardCoreTelemetryConstants.MeterName)
            .AddInstrumentation(serviceProvider => new TenantMetrics(
                serviceProvider.GetRequiredService<IShellHost>(),
                serviceProvider.GetRequiredService<IMeterFactory>()));
    }

    /// <summary>
    /// Adds the name of their tenant to the log records.
    /// </summary>
    /// <param name="builder">The <see cref="LoggerProviderBuilder"/> to configure.</param>
    /// <remarks>Call it before adding exporters, so that the log records are enriched before being exported.</remarks>
    public static LoggerProviderBuilder AddOrchardCoreInstrumentation(this LoggerProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddProcessor(new TenantLogRecordProcessor());
    }
}
