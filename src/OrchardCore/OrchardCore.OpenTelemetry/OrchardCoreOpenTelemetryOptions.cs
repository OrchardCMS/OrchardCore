using OpenTelemetry.Exporter;

namespace OrchardCore.Telemetry;

/// <summary>
/// The OpenTelemetry options of the application, bound from the 'OrchardCore:OpenTelemetry' configuration section.
/// </summary>
public sealed class OrchardCoreOpenTelemetryOptions
{
    /// <summary>
    /// The name of the configuration section the options are bound from.
    /// </summary>
    public const string ConfigurationSectionName = "OrchardCore:OpenTelemetry";

    /// <summary>
    /// Gets or sets whether OpenTelemetry is configured. Defaults to <see langword="true"/>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the 'service.name' resource attribute. Defaults to the application name.
    /// The 'OTEL_SERVICE_NAME' environment variable takes precedence over this value.
    /// </summary>
    public string ServiceName { get; set; }

    /// <summary>
    /// Gets or sets the 'service.instance.id' resource attribute that identifies this instance of the application,
    /// e.g. in a web farm. Defaults to the machine name followed by the process id.
    /// </summary>
    public string ServiceInstanceId { get; set; }

    /// <summary>
    /// Gets the tracing options.
    /// </summary>
    public OpenTelemetryTracingOptions Tracing { get; } = new();

    /// <summary>
    /// Gets the metrics options.
    /// </summary>
    public OpenTelemetryMetricsOptions Metrics { get; } = new();

    /// <summary>
    /// Gets the logging options.
    /// </summary>
    public OpenTelemetryLoggingOptions Logging { get; } = new();

    /// <summary>
    /// Gets the options of the OTLP exporter.
    /// </summary>
    public OpenTelemetryOtlpOptions Otlp { get; } = new();
}

/// <summary>
/// The OpenTelemetry tracing options.
/// </summary>
public sealed class OpenTelemetryTracingOptions
{
    /// <summary>
    /// Gets or sets whether traces are collected. Defaults to <see langword="true"/>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the ratio, from 0 to 1, of the traces that are sampled. A trace started by a sampled parent,
    /// e.g. in another service, is always sampled. When not set, all traces are sampled.
    /// </summary>
    public double? SamplingRatio { get; set; }

    /// <summary>
    /// Gets or sets the names of additional <see cref="System.Diagnostics.ActivitySource"/> to collect traces from.
    /// </summary>
    public string[] Sources { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the spans of the database operations include their SQL text, in 'db.query.text'.
    /// The commands run by YesSql are parameterized, so their text holds no values. Defaults to <see langword="true"/>.
    /// </summary>
    public bool IncludeDatabaseQueryText { get; set; } = true;

    /// <summary>
    /// Gets or sets the duration from which the rendering of a shape is traced with its own span.
    /// Defaults to 50 milliseconds. <see cref="TimeSpan.Zero"/> traces every shape.
    /// </summary>
    public TimeSpan SlowShapeThreshold { get; set; } = TimeSpan.FromMilliseconds(50);
}

/// <summary>
/// The OpenTelemetry metrics options.
/// </summary>
public sealed class OpenTelemetryMetricsOptions
{
    /// <summary>
    /// Gets or sets whether metrics are collected. Defaults to <see langword="true"/>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the HTTP server metrics are tagged with the name of the tenant that served the request.
    /// Defaults to <see langword="true"/>. Disable it to limit the number of metric series with many tenants.
    /// </summary>
    public bool IncludeTenantName { get; set; } = true;

    /// <summary>
    /// Gets or sets the names of additional <see cref="System.Diagnostics.Metrics.Meter"/> to collect metrics from.
    /// </summary>
    public string[] Meters { get; set; } = [];
}

/// <summary>
/// The OpenTelemetry logging options.
/// </summary>
public sealed class OpenTelemetryLoggingOptions
{
    /// <summary>
    /// Gets or sets whether logs are collected. Defaults to <see langword="true"/>.
    /// </summary>
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// The options of the OTLP exporter.
/// </summary>
public sealed class OpenTelemetryOtlpOptions
{
    /// <summary>
    /// Gets or sets the endpoint of the OTLP receiver, e.g. 'http://localhost:4317'. When neither this value nor the
    /// 'OTEL_EXPORTER_OTLP_ENDPOINT' configuration value is set, the telemetry is not exported over OTLP.
    /// </summary>
    public string Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the protocol used to export to <see cref="Endpoint"/>. Defaults to <see cref="OtlpExportProtocol.Grpc"/>.
    /// </summary>
    public OtlpExportProtocol Protocol { get; set; } = OtlpExportProtocol.Grpc;
}
