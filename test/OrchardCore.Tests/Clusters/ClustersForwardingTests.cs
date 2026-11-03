using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using global::Microsoft.Extensions.Configuration;
using global::Microsoft.Extensions.DependencyInjection;
using global::Microsoft.Extensions.Hosting;
using OrchardCore.Clusters;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using Yarp.ReverseProxy.Forwarder;

namespace OrchardCore.Tests.Clusters;

/// <summary>
/// Runs a request through the tenant container middleware and the actual YARP pipeline, the backends being replaced by <see cref="BackendHandler"/>.
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
        edge.ShellHost.Verify(host => host.GetScopeAsync(It.IsAny<ShellSettings>()), Times.Never);
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
        edge.ShellHost.Verify(host => host.GetScopeAsync(It.IsAny<ShellSettings>()), Times.Never);
    }

    [Fact]
    public async Task Forward_RequestWithSecret_RunsTheTenantWithoutForwarding()
    {
        await using var edge = await Edge.StartAsync();

        await Assert.ThrowsAsync<TenantRunException>(() => edge.SendAsync("http://tenant.example/", Secret));

        Assert.Empty(edge.Backend.Requests);
    }

    [Fact]
    public async Task Forward_ClusteringDisabled_RunsTheTenantWithoutForwarding()
    {
        await using var edge = await Edge.StartAsync(enabled: false);

        await Assert.ThrowsAsync<TenantRunException>(() => edge.SendAsync("http://tenant.example/"));

        Assert.Empty(edge.Backend.Requests);
    }

    [Fact]
    public async Task Forward_SecretReloaded_IsUsedOnNextRequest()
    {
        await using var edge = await Edge.StartAsync();

        edge.Configuration["OrchardCore:Clusters:ProxySecret"] = "rotated";
        edge.Configuration.Reload();

        await edge.SendAsync("http://tenant.example/");
        await Assert.ThrowsAsync<TenantRunException>(() => edge.SendAsync("http://tenant.example/", "rotated"));

        Assert.Equal("rotated", Assert.Single(edge.Backend.Requests).ProxyHeader);
    }

    [Fact]
    public async Task Forward_TenantInNoCluster_ReturnsServiceUnavailable()
    {
        await using var edge = await Edge.StartAsync(tenantInCluster: false);

        var response = await edge.SendAsync("http://tenant.example/");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Empty(edge.Backend.Requests);
    }

    private sealed class Edge : IAsyncDisposable
    {
        private readonly IHost _host;

        private Edge(IHost host, IConfigurationRoot configuration, BackendHandler backend, Mock<IShellHost> shellHost)
        {
            _host = host;
            Configuration = configuration;
            Backend = backend;
            ShellHost = shellHost;
        }

        public IConfigurationRoot Configuration { get; }

        public BackendHandler Backend { get; }

        public Mock<IShellHost> ShellHost { get; }

        public static async Task<Edge> StartAsync(bool enabled = true, bool tenantInCluster = true)
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
            shellHost.Setup(host => host.GetScopeAsync(settings)).ThrowsAsync(new TenantRunException());

            var runningShellTable = new Mock<IRunningShellTable>();
            runningShellTable
                .Setup(table => table.Match(It.IsAny<HostString>(), It.IsAny<PathString>(), true))
                .Returns(settings);

            var backend = new BackendHandler();

            var host = await new HostBuilder()
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
                        app.UseMiddleware<ModularTenantContainerMiddleware>();
                        app.UseEndpoints(endpoints => endpoints.MapReverseProxy(pipeline => pipeline.UseTenantClusters()));
                    }))
                .StartAsync(TestContext.Current.CancellationToken);

            return new Edge(host, configuration, backend, shellHost);
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

    private sealed class TenantRunException : Exception;

    private sealed record ForwardedRequest(string Uri, string Host, string ProxyHeader);

    private sealed class BackendHandler : HttpMessageHandler
    {
        public List<ForwardedRequest> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
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
