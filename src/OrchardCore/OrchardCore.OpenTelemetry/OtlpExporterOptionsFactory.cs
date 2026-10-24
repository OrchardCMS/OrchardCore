using Microsoft.Extensions.Configuration;
using OpenTelemetry.Exporter;

namespace OrchardCore.Telemetry;

/// <summary>
/// Creates the options of the OTLP exporter of a signal, from the 'OrchardCore:OpenTelemetry:Otlp' section or from
/// the standard 'OTEL_EXPORTER_OTLP_*' configuration values.
/// </summary>
internal static class OtlpExporterOptionsFactory
{
    public const string Traces = "traces";
    public const string Metrics = "metrics";
    public const string Logs = "logs";

    /// <summary>
    /// Returns the options of the OTLP exporter of the signal, or <see langword="null"/> when no endpoint is configured.
    /// </summary>
    public static OtlpExporterOptions Create(OrchardCoreOpenTelemetryOptions options, IConfiguration configuration, string signal)
    {
        var endpoint = options.Otlp.Endpoint;
        var protocol = options.Otlp.Protocol;

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            endpoint = configuration?["OTEL_EXPORTER_OTLP_ENDPOINT"];
            protocol = configuration?["OTEL_EXPORTER_OTLP_PROTOCOL"] == "http/protobuf"
                ? OtlpExportProtocol.HttpProtobuf
                : OtlpExportProtocol.Grpc;
        }

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return null;
        }

        // The base endpoint is shared by all signals, each one being exported to its own path over HTTP.
        if (protocol == OtlpExportProtocol.HttpProtobuf)
        {
            endpoint = $"{endpoint.TrimEnd('/')}/v1/{signal}";
        }

        var exporterOptions = new OtlpExporterOptions
        {
            Endpoint = new Uri(endpoint),
            Protocol = protocol,
        };

        var headers = configuration?["OTEL_EXPORTER_OTLP_HEADERS"];
        if (!string.IsNullOrWhiteSpace(headers))
        {
            exporterOptions.Headers = headers;
        }

        return exporterOptions;
    }
}
