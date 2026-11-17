using OrchardCore.Telemetry;

namespace Microsoft.Extensions.Configuration;

/// <summary>
/// Extensions reading the OTLP settings of the application from its configuration.
/// </summary>
public static class OtlpConfigurationExtensions
{
    /// <summary>
    /// Returns whether an OTLP endpoint is configured, in 'OrchardCore:OpenTelemetry:Otlp:Endpoint' or in one of the
    /// standard 'OTEL_EXPORTER_OTLP_ENDPOINT' and 'OTEL_EXPORTER_OTLP_{SIGNAL}_ENDPOINT' configuration values. It tells
    /// whether calling 'AddOpenTelemetry()' would export anything when no other exporter is added.
    /// </summary>
    /// <param name="configuration">The configuration of the application.</param>
    public static bool HasOtlpEndpoint(this IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return OtlpExporterOptionsFactory.HasEndpoint(configuration);
    }
}
