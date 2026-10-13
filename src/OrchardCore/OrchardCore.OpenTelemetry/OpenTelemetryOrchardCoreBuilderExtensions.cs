using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrchardCore.Telemetry;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions configuring OpenTelemetry for an Orchard Core application.
/// </summary>
public static class OpenTelemetryOrchardCoreBuilderExtensions
{
    // Database clients emitting their own metrics, such as the use of their connection pools. Their own traces would
    // duplicate the ones of the database operations that Orchard Core traces for every database provider.
    private static readonly string[] s_databaseMeters = ["Npgsql", "MySqlConnector"];

    // Client libraries used by Orchard Core modules that trace their own operations: the Elasticsearch client, MailKit
    // for SMTP, and the Azure SDK clients (Blob Storage, Key Vault, Communication Services, AI Search).
    private static readonly string[] s_clientLibrarySources = ["Elastic.Transport", "MailKit.Net.SmtpClient", "Azure.*"];

    // The switch enabling the activity sources of the Azure SDK clients, which are experimental and off by default.
    private const string AzureActivitySourceSwitch = "Azure.Experimental.EnableActivitySource";

    // The meter built in .NET that the ASP.NET Core and HttpClient instrumentations don't already collect.
    private static readonly string[] s_runtimeMeters = ["System.Runtime"];

    /// <summary>
    /// Configures OpenTelemetry to collect the traces, metrics and logs of the application, from Orchard Core, ASP.NET Core,
    /// HttpClient, the .NET runtime and the database clients, tagged with their tenant and with resource attributes
    /// identifying this instance of the application. The options are read from the 'OrchardCore:OpenTelemetry' section,
    /// and the telemetry is exported over OTLP when an endpoint is configured in 'OrchardCore:OpenTelemetry:Otlp:Endpoint'
    /// or in the standard 'OTEL_EXPORTER_OTLP_ENDPOINT' configuration value.
    /// </summary>
    /// <param name="builder">The <see cref="OrchardCoreBuilder"/>.</param>
    /// <param name="configure">Optional callback to further configure OpenTelemetry, e.g. to add other instrumentations or exporters.</param>
    public static OrchardCoreBuilder AddOpenTelemetry(this OrchardCoreBuilder builder, Action<OpenTelemetryBuilder> configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var services = builder.ApplicationServices;

        // Registering the exporters twice would export everything twice.
        if (services.Any(descriptor => descriptor.ServiceType == typeof(OpenTelemetryMarker)))
        {
            return builder;
        }

        services.AddSingleton<OpenTelemetryMarker>();

        // The configuration is only available from the service provider, so every choice depending on it is deferred.
        var openTelemetry = services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddDetector(serviceProvider => new OrchardCoreResourceDetector(
                    GetOptions(serviceProvider),
                    serviceProvider.GetService<IHostEnvironment>()))
                // Added again last, so that the standard 'OTEL_SERVICE_NAME' and 'OTEL_RESOURCE_ATTRIBUTES' take precedence.
                .AddEnvironmentVariableDetector());

        openTelemetry.WithTracing(tracing => tracing
            .AddOrchardCoreInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .SetSampler(serviceProvider => CreateSampler(GetOptions(serviceProvider))));

        services.ConfigureOpenTelemetryTracerProvider((serviceProvider, tracing) =>
        {
            var options = GetOptions(serviceProvider);
            tracing.AddSource(options.Tracing.Sources);

            if (options.Tracing.IncludeClientLibraries)
            {
                tracing.AddSource(s_clientLibrarySources);
                EnableAzureActivitySources();
            }
        });

        openTelemetry.WithMetrics(metrics => metrics
            .AddOrchardCoreInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter(s_runtimeMeters)
            .AddMeter(s_databaseMeters));

        services.ConfigureOpenTelemetryMeterProvider((serviceProvider, metrics) =>
        {
            var options = GetOptions(serviceProvider);
            if (!options.Enabled || !options.Metrics.Enabled)
            {
                metrics.AddView(_ => MetricStreamConfiguration.Drop);

                return;
            }

            metrics.AddMeter(options.Metrics.Meters);
        });

        openTelemetry.WithLogging(
            logging => logging.AddOrchardCoreInstrumentation(),
            logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

        services
            .AddOptions<LoggerFilterOptions>()
            .Configure<IOptions<OrchardCoreOpenTelemetryOptions>>((filters, options) =>
            {
                if (!options.Value.Enabled || !options.Value.Logging.Enabled)
                {
                    filters.AddFilter<OpenTelemetryLoggerProvider>(null, LogLevel.None);
                }
            });

        configure?.Invoke(openTelemetry);

        // The exporters are added last, so that the telemetry is enriched before being exported.
        services.ConfigureOpenTelemetryTracerProvider((serviceProvider, tracing) =>
        {
            if (CreateOtlpExporterOptions(serviceProvider, OtlpExporterOptionsFactory.Traces, options => options.Tracing.Enabled) is { } exporterOptions)
            {
                tracing.AddProcessor(new BatchActivityExportProcessor(new OtlpTraceExporter(exporterOptions)));
            }
        });

        services.ConfigureOpenTelemetryMeterProvider((serviceProvider, metrics) =>
        {
            if (CreateOtlpExporterOptions(serviceProvider, OtlpExporterOptionsFactory.Metrics, options => options.Metrics.Enabled) is { } exporterOptions)
            {
                // Reads the standard 'OTEL_METRIC_EXPORT_INTERVAL' and 'OTEL_METRIC_EXPORT_TIMEOUT' environment variables.
                var readerOptions = new PeriodicExportingMetricReaderOptions();

                metrics.AddReader(new PeriodicExportingMetricReader(
                    new OtlpMetricExporter(exporterOptions),
                    readerOptions.ExportIntervalMilliseconds ?? 60_000,
                    readerOptions.ExportTimeoutMilliseconds ?? 30_000));
            }
        });

        services.ConfigureOpenTelemetryLoggerProvider((serviceProvider, logging) =>
        {
            if (CreateOtlpExporterOptions(serviceProvider, OtlpExporterOptionsFactory.Logs, options => options.Logging.Enabled) is { } exporterOptions)
            {
                logging.AddProcessor(new BatchLogRecordExportProcessor(new OtlpLogExporter(exporterOptions)));
            }
        });

        return builder;
    }

    private sealed class OpenTelemetryMarker;

    private static OrchardCoreOpenTelemetryOptions GetOptions(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<IOptions<OrchardCoreOpenTelemetryOptions>>().Value;
    }

    // Enables the activity sources of the Azure SDK clients, unless the application already decided with the
    // 'Azure.Experimental.EnableActivitySource' switch or the 'AZURE_EXPERIMENTAL_ENABLE_ACTIVITY_SOURCE' variable.
    private static void EnableAzureActivitySources()
    {
        if (AppContext.TryGetSwitch(AzureActivitySourceSwitch, out _) ||
            System.Environment.GetEnvironmentVariable("AZURE_EXPERIMENTAL_ENABLE_ACTIVITY_SOURCE") is not null)
        {
            return;
        }

        AppContext.SetSwitch(AzureActivitySourceSwitch, true);
    }

    private static Sampler CreateSampler(OrchardCoreOpenTelemetryOptions options)
    {
        if (!options.Enabled || !options.Tracing.Enabled)
        {
            return new AlwaysOffSampler();
        }

        return options.Tracing.SamplingRatio is { } samplingRatio
            ? new ParentBasedSampler(new TraceIdRatioBasedSampler(samplingRatio))
            : new ParentBasedSampler(new AlwaysOnSampler());
    }

    private static OtlpExporterOptions CreateOtlpExporterOptions(
        IServiceProvider serviceProvider,
        string signal,
        Func<OrchardCoreOpenTelemetryOptions, bool> isSignalEnabled)
    {
        var options = GetOptions(serviceProvider);
        if (!options.Enabled || !isSignalEnabled(options))
        {
            return null;
        }

        return OtlpExporterOptionsFactory.Create(options, serviceProvider.GetService<IConfiguration>(), signal);
    }
}
