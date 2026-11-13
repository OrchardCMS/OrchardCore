using System.Globalization;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;

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

    private const string ConfigurationPrefix = "OTEL_EXPORTER_OTLP_";

    /// <summary>
    /// Returns whether an OTLP endpoint is configured, for all signals or for one of them.
    /// </summary>
    public static bool HasEndpoint(IConfiguration configuration)
    {
        string[] keys =
        [
            $"{OrchardCoreOpenTelemetryOptions.ConfigurationSectionName}:Otlp:Endpoint",
            GetKey("ENDPOINT"),
            GetKey("ENDPOINT", Traces),
            GetKey("ENDPOINT", Metrics),
            GetKey("ENDPOINT", Logs),
        ];

        return keys.Any(key => !string.IsNullOrWhiteSpace(configuration[key]));
    }

    /// <summary>
    /// Returns the options of the OTLP exporter of the signal, or <see langword="null"/> when no endpoint is configured.
    /// </summary>
    public static OtlpExporterOptions Create(OrchardCoreOpenTelemetryOptions options, IConfiguration configuration, string signal)
    {
        var endpoint = options.Otlp.Endpoint;
        var isSignalEndpoint = false;

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            endpoint = configuration?[GetKey("ENDPOINT", signal)];
            isSignalEndpoint = !string.IsNullOrWhiteSpace(endpoint);

            if (!isSignalEndpoint)
            {
                endpoint = configuration?[GetKey("ENDPOINT")];
            }
        }

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return null;
        }

        var protocol = options.Otlp.Protocol ?? (string.Equals(GetValue(configuration, "PROTOCOL", signal), "http/protobuf", StringComparison.OrdinalIgnoreCase)
            ? OtlpExportProtocol.HttpProtobuf
            : OtlpExportProtocol.Grpc);

        // The base endpoint is shared by all signals, each one being exported to its own path over HTTP. The endpoint
        // of a signal is used as is.
        if (protocol == OtlpExportProtocol.HttpProtobuf && !isSignalEndpoint)
        {
            endpoint = $"{endpoint.TrimEnd('/')}/v1/{signal}";
        }

        // An endpoint without a scheme, e.g. 'localhost:4317', would be accepted but nothing would ever be exported.
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri) || (endpointUri.Scheme != Uri.UriSchemeHttp && endpointUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException($"The OTLP endpoint of the {signal} is not an absolute HTTP or HTTPS URI, e.g. 'http://localhost:4317'.");
        }

        // Reads the standard environment variables, which the configuration values below take precedence over.
        var exporterOptions = new OtlpExporterOptions
        {
            Endpoint = endpointUri,
            Protocol = protocol,
        };

        var headers = GetValue(configuration, "HEADERS", signal);
        if (!string.IsNullOrWhiteSpace(headers))
        {
            exporterOptions.Headers = headers;
        }

        if (int.TryParse(GetValue(configuration, "TIMEOUT", signal), NumberStyles.None, CultureInfo.InvariantCulture, out var timeout))
        {
            exporterOptions.TimeoutMilliseconds = timeout;
        }

        var compression = GetValue(configuration, "COMPRESSION", signal);
        if (string.Equals(compression, "gzip", StringComparison.OrdinalIgnoreCase))
        {
            exporterOptions.Compression = OtlpExportCompression.GZip;
        }
        else if (string.Equals(compression, "none", StringComparison.OrdinalIgnoreCase))
        {
            exporterOptions.Compression = OtlpExportCompression.None;
        }

        return exporterOptions;
    }

    /// <summary>
    /// Returns the temporality preference of the exported metrics, from the standard configuration value.
    /// </summary>
    public static MetricReaderTemporalityPreference GetTemporalityPreference(IConfiguration configuration)
    {
        return Enum.TryParse<MetricReaderTemporalityPreference>(configuration?[GetKey("METRICS_TEMPORALITY_PREFERENCE")], ignoreCase: true, out var preference)
            ? preference
            : MetricReaderTemporalityPreference.Cumulative;
    }

    /// <summary>
    /// Returns whether the standard configuration value asks for the histograms to be exported with exponential buckets.
    /// </summary>
    public static bool UsesExponentialHistograms(IConfiguration configuration)
    {
        return string.Equals(configuration?[GetKey("METRICS_DEFAULT_HISTOGRAM_AGGREGATION")], "base2_exponential_bucket_histogram", StringComparison.OrdinalIgnoreCase);
    }

    // The value of a signal takes precedence over the one shared by all signals.
    private static string GetValue(IConfiguration configuration, string name, string signal)
    {
        var value = configuration?[GetKey(name, signal)];

        return string.IsNullOrWhiteSpace(value) ? configuration?[GetKey(name)] : value;
    }

    private static string GetKey(string name, string signal = null)
    {
        return signal is null
            ? $"{ConfigurationPrefix}{name}"
            : $"{ConfigurationPrefix}{signal.ToUpperInvariant()}_{name}";
    }
}
