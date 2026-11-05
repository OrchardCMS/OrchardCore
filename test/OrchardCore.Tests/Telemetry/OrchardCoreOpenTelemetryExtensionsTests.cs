using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Telemetry;

namespace OrchardCore.Tests.Telemetry;

public class OrchardCoreOpenTelemetryExtensionsTests
{
    [Theory]
    [InlineData("Elastic.Transport")]
    [InlineData("MailKit.Net.SmtpClient")]
    [InlineData("Npgsql")]
    [InlineData("Azure.Storage.Blobs.BlobContainerClient")]
    public void AddOpenTelemetry_ConfiguredSources_SamplesTheirTraces(string sourceName)
    {
        var builder = CreateBuilder(new()
        {
            ["OrchardCore:OpenTelemetry:Tracing:Sources:0"] = "Elastic.Transport",
            ["OrchardCore:OpenTelemetry:Tracing:Sources:1"] = "MailKit.Net.SmtpClient",
            ["OrchardCore:OpenTelemetry:Tracing:Sources:2"] = "Npgsql",
            ["OrchardCore:OpenTelemetry:Tracing:Sources:3"] = "Azure.*",
        });

        new OrchardCoreBuilder(builder.Services).AddOpenTelemetry();

        using var host = builder.Build();
        host.Services.GetRequiredService<TracerProvider>();

        using var source = new ActivitySource(sourceName);
        using var activity = source.StartActivity("Operation");

        Assert.True(activity?.Recorded);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void AddOpenTelemetry_EnabledSetting_SamplesTraces(string enabled, bool sampled)
    {
        // A source only listened to by this provider, unlike the 'OrchardCore' source shared by concurrent tests.
        var sourceName = Guid.NewGuid().ToString("n");
        var builder = CreateBuilder(new()
        {
            ["OrchardCore:OpenTelemetry:Enabled"] = enabled,
            ["OrchardCore:OpenTelemetry:Tracing:Sources:0"] = sourceName,
        });

        new OrchardCoreBuilder(builder.Services).AddOpenTelemetry();

        using var host = builder.Build();
        host.Services.GetRequiredService<TracerProvider>();

        using var source = new ActivitySource(sourceName);
        using var activity = source.StartActivity("Operation");

        // A dropped activity may still be created to propagate the trace context, but it isn't recorded.
        Assert.Equal(sampled, activity?.Recorded ?? false);
    }

    [Theory]
    [InlineData("Elastic.Transport")]
    [InlineData("MailKit.Net.SmtpClient")]
    [InlineData("Npgsql")]
    [InlineData("Azure.Storage.Blobs.BlobContainerClient")]
    public void AddOpenTelemetry_Default_DoesNotCollectOtherLibraries(string sourceName)
    {
        var builder = CreateBuilder([]);

        new OrchardCoreBuilder(builder.Services).AddOpenTelemetry();

        using var host = builder.Build();
        host.Services.GetRequiredService<TracerProvider>();

        using var source = new ActivitySource(sourceName);
        using var activity = source.StartActivity("Operation");

        Assert.Null(activity);
        Assert.False(AppContext.TryGetSwitch("Azure.Experimental.EnableActivitySource", out _));
    }

    [Theory]
    [InlineData("true", null, "always_off", false)]
    [InlineData("true", null, "always_on", true)]
    [InlineData("true", null, null, true)]
    [InlineData("true", "1", "always_off", true)]
    [InlineData("false", null, "always_on", false)]
    public void AddOpenTelemetry_StandardSampler_AppliesWhenNotConfigured(string enabled, string samplingRatio, string standardSampler, bool sampled)
    {
        var sourceName = Guid.NewGuid().ToString("n");
        var builder = CreateBuilder(new()
        {
            ["OrchardCore:OpenTelemetry:Enabled"] = enabled,
            ["OrchardCore:OpenTelemetry:Tracing:SamplingRatio"] = samplingRatio,
            ["OrchardCore:OpenTelemetry:Tracing:Sources:0"] = sourceName,
            ["OTEL_TRACES_SAMPLER"] = standardSampler,
        });

        new OrchardCoreBuilder(builder.Services).AddOpenTelemetry();

        using var host = builder.Build();
        host.Services.GetRequiredService<TracerProvider>();

        using var source = new ActivitySource(sourceName);
        using var activity = source.StartActivity("Operation");

        Assert.Equal(sampled, activity?.Recorded ?? false);
    }

    [Fact]
    public void AddOpenTelemetry_SlowShapeThreshold_SetsTheTelemetryOptions()
    {
        var builder = CreateBuilder(new()
        {
            ["OrchardCore:OpenTelemetry:Tracing:SlowShapeThreshold"] = "00:00:01",
        });

        new OrchardCoreBuilder(builder.Services).AddOpenTelemetry();

        using var host = builder.Build();

        Assert.Equal(TimeSpan.FromSeconds(1), host.Services.GetRequiredService<IOptions<OrchardCoreTelemetryOptions>>().Value.SlowShapeThreshold);
    }

    [Fact]
    public void AddOpenTelemetry_CalledTwice_RegistersOnce()
    {
        var builder = CreateBuilder([]);
        var orchardCoreBuilder = new OrchardCoreBuilder(builder.Services);

        orchardCoreBuilder.AddOpenTelemetry();
        var servicesCount = builder.Services.Count;
        orchardCoreBuilder.AddOpenTelemetry();

        Assert.Equal(servicesCount, builder.Services.Count);
        Assert.Single(builder.Services, descriptor => descriptor.ImplementationType == typeof(TenantTelemetryStartupFilter));
    }

    [Fact]
    public void AddOpenTelemetry_WithConfiguration_IdentifiesInstanceInResource()
    {
        var builder = CreateBuilder(new()
        {
            ["OrchardCore:OpenTelemetry:ServiceName"] = "cms",
            ["OrchardCore:OpenTelemetry:ServiceInstanceId"] = "web-1",
        });

        new OrchardCoreBuilder(builder.Services).AddOpenTelemetry();

        using var host = builder.Build();
        var attributes = host.Services.GetRequiredService<TracerProvider>().GetResource().Attributes.ToDictionary();

        Assert.Equal("cms", attributes["service.name"]);
        Assert.Equal("web-1", attributes["service.instance.id"]);
        Assert.Equal(System.Environment.MachineName, attributes["host.name"]);
        Assert.Equal((long)System.Environment.ProcessId, attributes["process.pid"]);
        Assert.True(attributes.ContainsKey("orchardcore.version"));
    }

    [Fact]
    public void AddOpenTelemetry_WithoutConfiguration_UsesApplicationNameAndProcessInstanceId()
    {
        var builder = CreateBuilder([]);

        new OrchardCoreBuilder(builder.Services).AddOpenTelemetry();

        using var host = builder.Build();
        var attributes = host.Services.GetRequiredService<TracerProvider>().GetResource().Attributes.ToDictionary();

        Assert.Equal("TestApp", attributes["service.name"]);
        Assert.Equal($"{System.Environment.MachineName}-{System.Environment.ProcessId}", attributes["service.instance.id"]);
    }

    [Theory]
    [InlineData("http://collector:4317", "Grpc", "http://collector:4317/", OtlpExportProtocol.Grpc)]
    [InlineData("http://collector:4318/", "HttpProtobuf", "http://collector:4318/v1/traces", OtlpExportProtocol.HttpProtobuf)]
    public void OtlpExporterOptionsFactory_SectionEndpoint_TargetsSignal(string endpoint, string protocol, string expectedEndpoint, OtlpExportProtocol expectedProtocol)
    {
        var options = new OrchardCoreOpenTelemetryOptions();
        options.Otlp.Endpoint = endpoint;
        options.Otlp.Protocol = Enum.Parse<OtlpExportProtocol>(protocol);

        var exporterOptions = OtlpExporterOptionsFactory.Create(options, new ConfigurationBuilder().Build(), OtlpExporterOptionsFactory.Traces);

        Assert.Equal(new Uri(expectedEndpoint), exporterOptions.Endpoint);
        Assert.Equal(expectedProtocol, exporterOptions.Protocol);
    }

    [Fact]
    public void OtlpExporterOptionsFactory_StandardConfiguration_TargetsSignalWithHeaders()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://collector:4318",
                ["OTEL_EXPORTER_OTLP_PROTOCOL"] = "http/protobuf",
                ["OTEL_EXPORTER_OTLP_HEADERS"] = "api-key=secret",
            })
            .Build();

        var exporterOptions = OtlpExporterOptionsFactory.Create(new OrchardCoreOpenTelemetryOptions(), configuration, OtlpExporterOptionsFactory.Logs);

        Assert.Equal(new Uri("http://collector:4318/v1/logs"), exporterOptions.Endpoint);
        Assert.Equal(OtlpExportProtocol.HttpProtobuf, exporterOptions.Protocol);
        Assert.Equal("api-key=secret", exporterOptions.Headers);
    }

    [Fact]
    public void OtlpExporterOptionsFactory_SectionProtocolWithStandardEndpoint_UsesSectionProtocol()
    {
        var options = new OrchardCoreOpenTelemetryOptions();
        options.Otlp.Protocol = OtlpExportProtocol.HttpProtobuf;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://collector:4318",
            })
            .Build();

        var exporterOptions = OtlpExporterOptionsFactory.Create(options, configuration, OtlpExporterOptionsFactory.Traces);

        Assert.Equal(new Uri("http://collector:4318/v1/traces"), exporterOptions.Endpoint);
        Assert.Equal(OtlpExportProtocol.HttpProtobuf, exporterOptions.Protocol);
    }

    [Fact]
    public void OtlpExporterOptionsFactory_SectionEndpointWithStandardProtocol_UsesStandardProtocol()
    {
        var options = new OrchardCoreOpenTelemetryOptions();
        options.Otlp.Endpoint = "http://collector:4318";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["OTEL_EXPORTER_OTLP_PROTOCOL"] = "http/protobuf",
            })
            .Build();

        var exporterOptions = OtlpExporterOptionsFactory.Create(options, configuration, OtlpExporterOptionsFactory.Metrics);

        Assert.Equal(new Uri("http://collector:4318/v1/metrics"), exporterOptions.Endpoint);
        Assert.Equal(OtlpExportProtocol.HttpProtobuf, exporterOptions.Protocol);
    }

    [Fact]
    public void OtlpExporterOptionsFactory_NoEndpoint_ReturnsNull()
    {
        Assert.Null(OtlpExporterOptionsFactory.Create(new OrchardCoreOpenTelemetryOptions(), new ConfigurationBuilder().Build(), OtlpExporterOptionsFactory.Metrics));
    }

    [Fact]
    public void OtlpExporterOptionsFactory_SignalConfiguration_TakesPrecedenceForThatSignal()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://collector:4317",
                ["OTEL_EXPORTER_OTLP_HEADERS"] = "api-key=shared",
                ["OTEL_EXPORTER_OTLP_TIMEOUT"] = "1000",
                ["OTEL_EXPORTER_OTLP_TRACES_ENDPOINT"] = "http://traces:4318/custom/path",
                ["OTEL_EXPORTER_OTLP_TRACES_PROTOCOL"] = "http/protobuf",
                ["OTEL_EXPORTER_OTLP_TRACES_HEADERS"] = "api-key=traces",
                ["OTEL_EXPORTER_OTLP_TRACES_TIMEOUT"] = "2000",
                ["OTEL_EXPORTER_OTLP_TRACES_COMPRESSION"] = "gzip",
            })
            .Build();

        var traces = OtlpExporterOptionsFactory.Create(new OrchardCoreOpenTelemetryOptions(), configuration, OtlpExporterOptionsFactory.Traces);
        var logs = OtlpExporterOptionsFactory.Create(new OrchardCoreOpenTelemetryOptions(), configuration, OtlpExporterOptionsFactory.Logs);

        Assert.Equal(new Uri("http://traces:4318/custom/path"), traces.Endpoint);
        Assert.Equal(OtlpExportProtocol.HttpProtobuf, traces.Protocol);
        Assert.Equal("api-key=traces", traces.Headers);
        Assert.Equal(2000, traces.TimeoutMilliseconds);
        Assert.Equal(OtlpExportCompression.GZip, traces.Compression);

        Assert.Equal(new Uri("http://collector:4317"), logs.Endpoint);
        Assert.Equal(OtlpExportProtocol.Grpc, logs.Protocol);
        Assert.Equal("api-key=shared", logs.Headers);
        Assert.Equal(1000, logs.TimeoutMilliseconds);
    }

    [Fact]
    public void OtlpExporterOptionsFactory_OnlySignalEndpoint_ExportsOnlyThatSignal()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["OTEL_EXPORTER_OTLP_METRICS_ENDPOINT"] = "http://metrics:4317",
            })
            .Build();

        Assert.NotNull(OtlpExporterOptionsFactory.Create(new OrchardCoreOpenTelemetryOptions(), configuration, OtlpExporterOptionsFactory.Metrics));
        Assert.Null(OtlpExporterOptionsFactory.Create(new OrchardCoreOpenTelemetryOptions(), configuration, OtlpExporterOptionsFactory.Traces));
        Assert.Null(OtlpExporterOptionsFactory.Create(new OrchardCoreOpenTelemetryOptions(), configuration, OtlpExporterOptionsFactory.Logs));
    }

    [Fact]
    public void OtlpExporterOptionsFactory_SectionEndpointWithSignalEndpoint_UsesSectionEndpoint()
    {
        var options = new OrchardCoreOpenTelemetryOptions();
        options.Otlp.Endpoint = "http://collector:4318";
        options.Otlp.Protocol = OtlpExportProtocol.HttpProtobuf;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["OTEL_EXPORTER_OTLP_TRACES_ENDPOINT"] = "http://traces:4318/custom/path",
            })
            .Build();

        var exporterOptions = OtlpExporterOptionsFactory.Create(options, configuration, OtlpExporterOptionsFactory.Traces);

        Assert.Equal(new Uri("http://collector:4318/v1/traces"), exporterOptions.Endpoint);
    }

    [Theory]
    [InlineData("not a uri")]
    [InlineData("localhost:4317")]
    [InlineData("ftp://collector:4317")]
    public void OtlpExporterOptionsFactory_InvalidEndpoint_ThrowsClearException(string endpoint)
    {
        var options = new OrchardCoreOpenTelemetryOptions();
        options.Otlp.Endpoint = endpoint;

        var exception = Assert.Throws<InvalidOperationException>(() => OtlpExporterOptionsFactory.Create(options, new ConfigurationBuilder().Build(), OtlpExporterOptionsFactory.Traces));

        Assert.Contains("OTLP endpoint", exception.Message);
    }

    [Theory]
    [InlineData("OrchardCore:OpenTelemetry:Otlp:Endpoint", "http://collector:4317", true)]
    [InlineData("OTEL_EXPORTER_OTLP_ENDPOINT", "http://collector:4317", true)]
    [InlineData("OTEL_EXPORTER_OTLP_TRACES_ENDPOINT", "http://collector:4317", true)]
    [InlineData("OTEL_EXPORTER_OTLP_METRICS_ENDPOINT", "http://collector:4317", true)]
    [InlineData("OTEL_EXPORTER_OTLP_LOGS_ENDPOINT", "http://collector:4317", true)]
    [InlineData("OTEL_EXPORTER_OTLP_ENDPOINT", " ", false)]
    [InlineData("OTEL_EXPORTER_OTLP_PROTOCOL", "grpc", false)]
    public void HasOtlpEndpoint_Configuration_TellsWhetherAnEndpointIsConfigured(string key, string value, bool expected)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { [key] = value })
            .Build();

        Assert.Equal(expected, configuration.HasOtlpEndpoint());
    }

    [Theory]
    [InlineData("delta", MetricReaderTemporalityPreference.Delta)]
    [InlineData("LowMemory", MetricReaderTemporalityPreference.LowMemory)]
    [InlineData("cumulative", MetricReaderTemporalityPreference.Cumulative)]
    [InlineData("invalid", MetricReaderTemporalityPreference.Cumulative)]
    [InlineData(null, MetricReaderTemporalityPreference.Cumulative)]
    public void GetTemporalityPreference_StandardConfiguration_ReturnsPreference(string value, MetricReaderTemporalityPreference expected)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE"] = value })
            .Build();

        Assert.Equal(expected, OtlpExporterOptionsFactory.GetTemporalityPreference(configuration));
    }

    [Theory]
    [InlineData("base2_exponential_bucket_histogram", true)]
    [InlineData("BASE2_EXPONENTIAL_BUCKET_HISTOGRAM", true)]
    [InlineData("explicit_bucket_histogram", false)]
    [InlineData("invalid", false)]
    [InlineData(null, false)]
    public void UsesExponentialHistograms_StandardConfiguration_ReturnsPreference(string value, bool expected)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["OTEL_EXPORTER_OTLP_METRICS_DEFAULT_HISTOGRAM_AGGREGATION"] = value })
            .Build();

        Assert.Equal(expected, OtlpExporterOptionsFactory.UsesExponentialHistograms(configuration));
    }

    [Fact]
    public void TenantMetrics_Created_PublishesInstrumentsWithTheVersionOfTheMeter()
    {
        using var services = new ServiceCollection().AddMetrics().BuildServiceProvider();

        var versions = new ConcurrentQueue<string>();
        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, _) =>
            {
                if (instrument.Name == OrchardCoreTelemetryConstants.Instruments.Tenants)
                {
                    versions.Enqueue(instrument.Meter.Version);
                }
            },
        };
        listener.Start();

        _ = new TenantMetrics(Mock.Of<IShellHost>(), services.GetRequiredService<IMeterFactory>());

        Assert.NotEmpty(versions);
        Assert.All(versions, version => Assert.Equal(OrchardCoreTelemetry.Meter.Version, version));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TenantTelemetryMiddleware_AfterTenantResolved_TagsRequestWithTenantName(bool includeTenantNameInMetrics)
    {
        using var activity = new Activity("Request").Start();
        var metricsTags = new MetricsTagsFeature();

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IHttpActivityFeature>(new ActivityFeature { Activity = activity });
        httpContext.Features.Set<IHttpMetricsTagsFeature>(metricsTags);

        var options = new OrchardCoreOpenTelemetryOptions();
        options.Metrics.IncludeTenantName = includeTenantNameInMetrics;

        // The tenant is resolved by the Orchard Core pipeline, after the middleware has started.
        var middleware = new TenantTelemetryMiddleware(
            context =>
            {
                context.Features.Set(new ShellContextFeature
                {
                    ShellContext = new ShellContext { Settings = new ShellSettings { Name = "Tenant1" } },
                });

                return Task.CompletedTask;
            },
            Options.Create(options));

        await middleware.InvokeAsync(httpContext);

        Assert.Equal("Tenant1", activity.GetTagItem(OrchardCoreTelemetryConstants.Attributes.TenantName));
        Assert.Equal(
            includeTenantNameInMetrics,
            metricsTags.Tags.Contains(new(OrchardCoreTelemetryConstants.Attributes.TenantName, "Tenant1")));
    }

    [Fact]
    public async Task TenantTelemetryMiddleware_NoTenant_DoesNotTagRequest()
    {
        using var activity = new Activity("Request").Start();
        var metricsTags = new MetricsTagsFeature();

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IHttpActivityFeature>(new ActivityFeature { Activity = activity });
        httpContext.Features.Set<IHttpMetricsTagsFeature>(metricsTags);

        var middleware = new TenantTelemetryMiddleware(_ => Task.CompletedTask, Options.Create(new OrchardCoreOpenTelemetryOptions()));

        await middleware.InvokeAsync(httpContext);

        Assert.Null(activity.GetTagItem(OrchardCoreTelemetryConstants.Attributes.TenantName));
        Assert.Empty(metricsTags.Tags);
    }

    private static HostApplicationBuilder CreateBuilder(Dictionary<string, string> configuration)
    {
        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings { ApplicationName = "TestApp" });
        builder.Configuration.AddInMemoryCollection(configuration);

        return builder;
    }

    private sealed class ActivityFeature : IHttpActivityFeature
    {
        public Activity Activity { get; set; }
    }

    private sealed class MetricsTagsFeature : IHttpMetricsTagsFeature
    {
        public ICollection<KeyValuePair<string, object>> Tags { get; } = [];

        public bool MetricsDisabled { get; set; }
    }
}
