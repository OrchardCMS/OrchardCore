using System.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Exporter;
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
    [InlineData("Azure.Storage.Blobs.BlobContainerClient")]
    public void AddOpenTelemetry_Default_SamplesClientLibraryTraces(string sourceName)
    {
        var builder = CreateBuilder([]);

        new OrchardCoreBuilder(builder.Services).AddOpenTelemetry();

        using var host = builder.Build();
        host.Services.GetRequiredService<TracerProvider>();

        using var source = new ActivitySource(sourceName);
        using var activity = source.StartActivity("Operation");

        Assert.True(activity?.Recorded);
        Assert.True(AppContext.TryGetSwitch("Azure.Experimental.EnableActivitySource", out var enabled) && enabled);
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
    public void OtlpExporterOptionsFactory_NoEndpoint_ReturnsNull()
    {
        Assert.Null(OtlpExporterOptionsFactory.Create(new OrchardCoreOpenTelemetryOptions(), new ConfigurationBuilder().Build(), OtlpExporterOptionsFactory.Metrics));
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
