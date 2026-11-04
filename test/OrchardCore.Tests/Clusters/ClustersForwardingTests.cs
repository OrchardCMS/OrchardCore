using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using global::Microsoft.Extensions.Configuration;
using global::Microsoft.Extensions.DependencyInjection;
using global::Microsoft.Extensions.Hosting;
using global::Microsoft.Extensions.Options;
using OrchardCore.Clusters;
using OrchardCore.Environment.Shell;
using OrchardCore.Telemetry;
using Yarp.ReverseProxy.Forwarder;

namespace OrchardCore.Tests.Clusters;

/// <summary>
/// Runs a request through the routing middleware and the actual YARP pipeline, the tenant pipeline and the backends being replaced by test doubles.
/// </summary>
public class ClustersForwardingTests
{
    private const string Secret = "cluster-secret";

    [Fact]
    public async Task Forward_TenantRequest_ReachesItsClusterWithSecretAndOriginalRequest()
    {
        await using var edge = await Edge.StartAsync();

        var response = await edge.SendAsync("http://tenant.example/prefix/page?id=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var request = Assert.Single(edge.Backend.Requests);

        // The route references the other cluster, so this checks that the request has been reassigned.
        Assert.Equal("http://tenant-cluster.test/prefix/page?id=1", request.Uri);
        Assert.Equal("tenant.example", request.Host);
        Assert.Equal(Secret, request.ProxyHeader);
        Assert.False(edge.TenantRan);
    }

    [Fact]
    public async Task Forward_TenantRequest_TagsTheProxySpanWithClusterAndTenant()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Yarp.ReverseProxy",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };

        ActivitySource.AddActivityListener(listener);

        await using var edge = await Edge.StartAsync();

        await edge.SendAsync("http://tenant.example/");

        var span = edge.Backend.Activity;

        Assert.Equal("proxy.forwarder", span.OperationName);

        // An exporter sends all the values of an attribute, not only the first one.
        Assert.Equal("tenant-cluster", Assert.Single(span.TagObjects, tag => tag.Key == "proxy.cluster_id").Value);
        Assert.Equal("Tenant", span.GetTagItem(OrchardCoreTelemetryConstants.Attributes.TenantName));
    }

    [Theory]
    [InlineData("true")]
    [InlineData("guess")]
    public async Task Forward_SpoofedHeader_IsReplacedAndDoesNotRunTheTenant(string spoofed)
    {
        await using var edge = await Edge.StartAsync();

        var response = await edge.SendAsync("http://tenant.example/", spoofed);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Secret, Assert.Single(edge.Backend.Requests).ProxyHeader);
        Assert.False(edge.TenantRan);
    }

    [Fact]
    public async Task Forward_RequestWithSecret_RunsTheTenantWithoutTheHeader()
    {
        await using var edge = await Edge.StartAsync();

        await edge.SendAsync("http://tenant.example/", Secret);

        Assert.True(edge.TenantRan);
        Assert.Empty(edge.TenantProxyHeader);
        Assert.Empty(edge.Backend.Requests);
    }

    [Fact]
    public async Task Forward_ClusteringDisabled_RunsTheTenantWithoutTheHeader()
    {
        await using var edge = await Edge.StartAsync(enabled: false);

        await edge.SendAsync("http://tenant.example/", Secret);

        Assert.True(edge.TenantRan);
        Assert.Empty(edge.TenantProxyHeader);
        Assert.Empty(edge.Backend.Requests);
    }

    [Fact]
    public async Task Forward_NoClustersConfiguration_RunsTheTenantWithoutTheHeader()
    {
        var headerAtTenant = "not-called";
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[RequestHeaderNames.FromClustersProxy] = Secret;

        var monitor = new Mock<IOptionsMonitor<ClustersOptions>>();
        monitor.SetupGet(m => m.CurrentValue).Returns(new ClustersOptions());

        var middleware = new ClustersRoutingMiddleware(
            context =>
            {
                headerAtTenant = context.Request.Headers[RequestHeaderNames.FromClustersProxy].ToString();
                return Task.CompletedTask;
            },
            Mock.Of<IShellHost>(),
            Mock.Of<IRunningShellTable>(),
            monitor.Object);

        await middleware.Invoke(httpContext);

        Assert.Empty(headerAtTenant);
    }

    [Fact]
    public async Task Forward_SecretReloaded_IsUsedOnNextRequest()
    {
        await using var edge = await Edge.StartAsync();

        edge.Configuration["OrchardCore:Clusters:ProxySecret"] = "rotated";
        edge.Configuration.Reload();

        await edge.SendAsync("http://tenant.example/", Secret);

        Assert.False(edge.TenantRan);
        Assert.Equal("rotated", Assert.Single(edge.Backend.Requests).ProxyHeader);

        await edge.SendAsync("http://tenant.example/", "rotated");

        Assert.True(edge.TenantRan);
    }

    [Fact]
    public async Task Forward_TenantInNoCluster_ReturnsServiceUnavailable()
    {
        await using var edge = await Edge.StartAsync(tenantInCluster: false);

        var response = await edge.SendAsync("http://tenant.example/");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Empty(edge.Backend.Requests);
        Assert.False(edge.TenantRan);
    }

    [Fact]
    public async Task Forward_NoTenantMatches_ReturnsNotFound()
    {
        await using var edge = await Edge.StartAsync(tenantMatches: false);

        var response = await edge.SendAsync("http://unknown.example/");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(edge.Backend.Requests);
        Assert.False(edge.TenantRan);
    }

    [Fact]
    public async Task Forward_EndpointOfTheHost_IsNotForwarded()
    {
        await using var edge = await Edge.StartAsync();

        await edge.SendAsync("http://tenant.example/host-endpoint");

        Assert.Empty(edge.Backend.Requests);
        Assert.True(edge.TenantRan);
    }

    private sealed class Edge : IAsyncDisposable
    {
        private IHost _host;

        private Edge(IConfigurationRoot configuration, BackendHandler backend)
        {
            Configuration = configuration;
            Backend = backend;
        }

        public IConfigurationRoot Configuration { get; }

        public BackendHandler Backend { get; }

        public bool TenantRan { get; private set; }

        public string TenantProxyHeader { get; private set; }

        public static async Task<Edge> StartAsync(bool enabled = true, bool tenantInCluster = true, bool tenantMatches = true)
        {
            var settings = new ShellSettings { Name = "Tenant", VersionId = "tenant-identifier" };
            var slot = settings.GetClusterSlot();
            var tenantClusterSlot = tenantInCluster ? slot : slot + 1;

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["OrchardCore:Clusters:Enabled"] = enabled.ToString(),
                    ["OrchardCore:Clusters:ProxySecret"] = Secret,
                    ["OrchardCore:Clusters:Routes:RouteTemplate:ClusterId"] = "other-cluster",
                    ["OrchardCore:Clusters:Routes:RouteTemplate:Match:Path"] = "{**catch-all}",
                    ["OrchardCore:Clusters:Clusters:tenant-cluster:SlotRange:0"] = tenantClusterSlot.ToString(),
                    ["OrchardCore:Clusters:Clusters:tenant-cluster:SlotRange:1"] = tenantClusterSlot.ToString(),
                    ["OrchardCore:Clusters:Clusters:tenant-cluster:Destinations:destination:Address"] = "http://tenant-cluster.test/",
                    ["OrchardCore:Clusters:Clusters:other-cluster:SlotRange:0"] = (slot + 2).ToString(),
                    ["OrchardCore:Clusters:Clusters:other-cluster:SlotRange:1"] = (slot + 2).ToString(),
                    ["OrchardCore:Clusters:Clusters:other-cluster:Destinations:destination:Address"] = "http://other-cluster.test/",
                })
                .Build();

            var shellHost = new Mock<IShellHost>();
            shellHost.Setup(host => host.InitializeAsync()).Returns(Task.CompletedTask);

            var runningShellTable = new Mock<IRunningShellTable>();
            runningShellTable
                .Setup(table => table.Match(It.IsAny<HostString>(), It.IsAny<PathString>(), true))
                .Returns(tenantMatches ? settings : null);

            var backend = new BackendHandler();

            var edge = new Edge(configuration, backend);

            edge._host = await new HostBuilder()
                .ConfigureWebHost(webHost => webHost
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddRouting();
                        services.AddSingleton<global::Microsoft.Extensions.Configuration.IConfiguration>(configuration);
                        services.AddSingleton(shellHost.Object);
                        services.AddSingleton(runningShellTable.Object);
                        services.AddSingleton<IForwarderHttpClientFactory>(new BackendClientFactory(backend));
                        services
                            .AddReverseProxy()
                            .AddTenantClusters()
                            .LoadFromConfig(configuration.GetSection(ClustersOptions.ConfigurationSection));
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseTenantClusterRouting();

                        // Stands for 'UseOrchardCore()', a forwarded request should not reach it.
                        app.Use((HttpContext httpContext, RequestDelegate _) =>
                        {
                            edge.TenantRan = true;
                            edge.TenantProxyHeader = httpContext.Request.Headers[RequestHeaderNames.FromClustersProxy].ToString();

                            return Task.CompletedTask;
                        });

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/host-endpoint", () => "host");
                            endpoints.MapReverseProxy(pipeline => pipeline.UseTenantClusters());
                        });
                    }))
                .StartAsync(TestContext.Current.CancellationToken);

            return edge;
        }

        public Task<HttpResponseMessage> SendAsync(string url, string proxyHeader = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (proxyHeader is not null)
            {
                request.Headers.Add(RequestHeaderNames.FromClustersProxy, proxyHeader);
            }

            return _host.GetTestClient().SendAsync(request, TestContext.Current.CancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _host.StopAsync(TestContext.Current.CancellationToken);
            _host.Dispose();
        }
    }

    private sealed record ForwardedRequest(string Uri, string Host, string ProxyHeader);

    private sealed class BackendHandler : HttpMessageHandler
    {
        public List<ForwardedRequest> Requests { get; } = [];

        public Activity Activity { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Activity = Activity.Current;

            request.Headers.TryGetValues(RequestHeaderNames.FromClustersProxy, out var values);

            Requests.Add(new ForwardedRequest(request.RequestUri.ToString(), request.Headers.Host, string.Join(',', values ?? [])));

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    private sealed class BackendClientFactory : IForwarderHttpClientFactory
    {
        private readonly BackendHandler _handler;

        public BackendClientFactory(BackendHandler handler) => _handler = handler;

        public HttpMessageInvoker CreateClient(ForwarderHttpClientContext context) => new(_handler, disposeHandler: false);
    }
}
