using System.Diagnostics.Metrics;
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
    // The meter built in .NET that the ASP.NET Core and HttpClient instrumentations don't already collect.
    private static readonly string[] s_runtimeMeters = ["System.Runtime"];

    /// <summary>
    /// Configures OpenTelemetry to collect the traces, metrics and logs of the application, from Orchard Core, ASP.NET Core,
    /// HttpClient and the .NET runtime, tagged with their tenant and with resource attributes identifying this
    /// instance of the application. The options are read from the 'OrchardCore:OpenTelemetry' section, whose
    /// 'Tracing:Sources' and 'Metrics:Meters' settings collect the telemetry other libraries emit themselves,
    /// and the telemetry is exported over OTLP when an endpoint is configured in 'OrchardCore:OpenTelemetry:Otlp:Endpoint'
    /// or in the standard 'OTEL_EXPORTER_OTLP_ENDPOINT' and 'OTEL_EXPORTER_OTLP_{SIGNAL}_ENDPOINT' configuration values.
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
            .AddHttpClientInstrumentation());

        services.ConfigureOpenTelemetryTracerProvider((serviceProvider, tracing) =>
        {
            var options = GetOptions(serviceProvider);
            tracing.AddSource(options.Tracing.Sources);

            if (CreateSampler(options) is { } sampler)
            {
                tracing.SetSampler(sampler);
            }
        });

        openTelemetry.WithMetrics(metrics => metrics
            .AddOrchardCoreInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter(s_runtimeMeters));

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
                // Reads the standard 'OTEL_BSP_*' environment variables.
                var batchOptions = new BatchExportActivityProcessorOptions();

                tracing.AddProcessor(new BatchActivityExportProcessor(
                    new OtlpTraceExporter(exporterOptions),
                    batchOptions.MaxQueueSize,
                    batchOptions.ScheduledDelayMilliseconds,
                    batchOptions.ExporterTimeoutMilliseconds,
                    batchOptions.MaxExportBatchSize));
            }
        });

        services.ConfigureOpenTelemetryMeterProvider((serviceProvider, metrics) =>
        {
            if (CreateOtlpExporterOptions(serviceProvider, OtlpExporterOptionsFactory.Metrics, options => options.Metrics.Enabled) is { } exporterOptions)
            {
                var configuration = serviceProvider.GetService<IConfiguration>();

                // Reads the standard 'OTEL_METRIC_EXPORT_INTERVAL' and 'OTEL_METRIC_EXPORT_TIMEOUT' environment variables.
                var readerOptions = new PeriodicExportingMetricReaderOptions();

                metrics.AddReader(new PeriodicExportingMetricReader(
                    new OtlpMetricExporter(exporterOptions),
                    readerOptions.ExportIntervalMilliseconds ?? 60_000,
                    readerOptions.ExportTimeoutMilliseconds ?? 30_000)
                {
                    TemporalityPreference = OtlpExporterOptionsFactory.GetTemporalityPreference(configuration),
                });

                if (OtlpExporterOptionsFactory.UsesExponentialHistograms(configuration))
                {
                    metrics.AddView(instrument => IsHistogram(instrument) ? new Base2ExponentialBucketHistogramConfiguration() : null);
                }
            }
        });

        services.ConfigureOpenTelemetryLoggerProvider((serviceProvider, logging) =>
        {
            if (CreateOtlpExporterOptions(serviceProvider, OtlpExporterOptionsFactory.Logs, options => options.Logging.Enabled) is { } exporterOptions)
            {
                // Reads the standard 'OTEL_BLRP_*' environment variables.
                var batchOptions = new BatchExportLogRecordProcessorOptions();

                logging.AddProcessor(new BatchLogRecordExportProcessor(
                    new OtlpLogExporter(exporterOptions),
                    batchOptions.MaxQueueSize,
                    batchOptions.ScheduledDelayMilliseconds,
                    batchOptions.ExporterTimeoutMilliseconds,
                    batchOptions.MaxExportBatchSize));
            }
        });

        return builder;
    }

    private sealed class OpenTelemetryMarker;

    private static bool IsHistogram(Instrument instrument)
    {
        var type = instrument.GetType();

        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Histogram<>);
    }

    private static OrchardCoreOpenTelemetryOptions GetOptions(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<IOptions<OrchardCoreOpenTelemetryOptions>>().Value;
    }

    // Without a sampler, the SDK uses the standard 'OTEL_TRACES_SAMPLER' value, and otherwise samples all traces.
    private static Sampler CreateSampler(OrchardCoreOpenTelemetryOptions options)
    {
        if (!options.Enabled || !options.Tracing.Enabled)
        {
            return new AlwaysOffSampler();
        }

        return options.Tracing.SamplingRatio is { } samplingRatio
            ? new ParentBasedSampler(new TraceIdRatioBasedSampler(samplingRatio))
            : null;
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
