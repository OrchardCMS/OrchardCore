using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.Options;
using OrchardCore.Clusters;
using Yarp.ReverseProxy;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Model;

namespace OrchardCore.Tests.Clusters;

public class ClustersProxyTests
{
    [Fact]
    public async Task ConfigureRoute_RouteTemplate_KeepsOriginalHost()
    {
        var route = new RouteConfig { RouteId = ClustersOptions.RouteTemplate };

        var configured = await new ClustersProxyConfigFilter().ConfigureRouteAsync(route, null, TestContext.Current.CancellationToken);

        Assert.Contains(configured.Transforms, transform => transform.TryGetValue("RequestHeaderOriginalHost", out var value) && value == "True");
    }

    [Fact]
    public async Task ConfigureRoute_OtherRoute_IsUnchanged()
    {
        var route = new RouteConfig { RouteId = "other" };

        var configured = await new ClustersProxyConfigFilter().ConfigureRouteAsync(route, null, TestContext.Current.CancellationToken);

        Assert.Same(route, configured);
    }

    [Theory]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    public async Task MatcherPolicy_RouteTemplate_IsOnlySelectedWhenForwarding(bool enabled, bool fromProxy, bool expected)
    {
        var (policy, _) = CreatePolicy(enabled);
        var candidates = CreateCandidates(CreateProxyEndpoint(ClustersOptions.RouteTemplate));
        var httpContext = CreateHttpContext(fromProxy);

        await policy.ApplyAsync(httpContext, candidates);

        Assert.Equal(expected, candidates.IsValidCandidate(0));
    }

    [Theory]
    [InlineData("true")]
    [InlineData("")]
    [InlineData("secret2")]
    [InlineData("secret,secret")]
    public async Task MatcherPolicy_WrongSecret_IsSelectedForForwarding(string headerValue)
    {
        var (policy, _) = CreatePolicy(enabled: true);
        var candidates = CreateCandidates(CreateProxyEndpoint(ClustersOptions.RouteTemplate));
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[RequestHeaderNames.FromClustersProxy] = headerValue;

        await policy.ApplyAsync(httpContext, candidates);

        Assert.True(candidates.IsValidCandidate(0));
    }

    [Fact]
    public async Task MatcherPolicy_OptionsChange_IsAppliedOnNextRequest()
    {
        var (policy, options) = CreatePolicy(enabled: false);
        var endpoint = CreateProxyEndpoint(ClustersOptions.RouteTemplate);

        var disabled = CreateCandidates(endpoint);
        await policy.ApplyAsync(CreateHttpContext(fromProxy: false), disabled);
        Assert.False(disabled.IsValidCandidate(0));

        options.Enabled = true;

        var enabled = CreateCandidates(endpoint);
        await policy.ApplyAsync(CreateHttpContext(fromProxy: false), enabled);
        Assert.True(enabled.IsValidCandidate(0));
    }

    [Fact]
    public async Task MatcherPolicy_OtherEndpoints_AreKept()
    {
        var (policy, _) = CreatePolicy(enabled: false);
        var candidates = CreateCandidates(CreateProxyEndpoint("other"), new Endpoint(null, EndpointMetadataCollection.Empty, "plain"));

        await policy.ApplyAsync(CreateHttpContext(fromProxy: false), candidates);

        Assert.True(candidates.IsValidCandidate(0));
        Assert.True(candidates.IsValidCandidate(1));
    }

    [Fact]
    public void MatcherPolicy_AppliesToEndpoints_OnlyWithRouteTemplate()
    {
        var (policy, _) = CreatePolicy(enabled: true);

        Assert.True(policy.AppliesToEndpoints([CreateProxyEndpoint(ClustersOptions.RouteTemplate)]));
        Assert.False(policy.AppliesToEndpoints([CreateProxyEndpoint("other"), new Endpoint(null, EndpointMetadataCollection.Empty, "plain")]));
    }

    [Fact]
    public async Task Middleware_ClusterNotFound_ReturnsServiceUnavailable()
    {
        var lookup = new Mock<IProxyStateLookup>();
        var nextCalled = false;
        var middleware = new ClustersProxyMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            lookup.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set(new ClusterFeature { ClusterId = "removed" });

        await middleware.Invoke(httpContext);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task Middleware_NoClusterFeature_ReturnsServiceUnavailable()
    {
        var nextCalled = false;
        var middleware = new ClustersProxyMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            Mock.Of<IProxyStateLookup>());

        var httpContext = new DefaultHttpContext();

        await middleware.Invoke(httpContext);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, httpContext.Response.StatusCode);
    }

    private static (ClustersProxyMatcherPolicy Policy, ClustersOptions Options) CreatePolicy(bool enabled)
    {
        var options = new ClustersOptions { Enabled = enabled, ProxySecret = "secret" };
        var monitor = new Mock<IOptionsMonitor<ClustersOptions>>();
        monitor.SetupGet(m => m.CurrentValue).Returns(options);

        return (new ClustersProxyMatcherPolicy(monitor.Object), options);
    }

    private static DefaultHttpContext CreateHttpContext(bool fromProxy)
    {
        var httpContext = new DefaultHttpContext();

        if (fromProxy)
        {
            httpContext.Request.Headers[RequestHeaderNames.FromClustersProxy] = "secret";
        }

        return httpContext;
    }

    private static CandidateSet CreateCandidates(params Endpoint[] endpoints) =>
        new(endpoints, new RouteValueDictionary[endpoints.Length], new int[endpoints.Length]);

    private static Endpoint CreateProxyEndpoint(string routeId)
    {
        var route = new RouteModel(new RouteConfig { RouteId = routeId }, null, HttpTransformer.Empty);

        return new Endpoint(null, new EndpointMetadataCollection(route), routeId);
    }
}
