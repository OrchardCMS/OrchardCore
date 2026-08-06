using Microsoft.AspNetCore.Routing.Patterns;
using OrchardCore.Routing;

namespace OrchardCore.Tests.Routing;

public class ShellEndpointNameAddressSchemeTests
{
    [Fact]
    public void FindEndpoints_RouteEndpointAndPlaceholderShareName_ReturnsWithoutThrowing()
    {
        // Arrange
        var routeEndpoint = CreateRouteEndpoint("Api");
        var placeholder = CreatePlaceholderEndpoint("Api");
        var scheme = new ShellEndpointNameAddressScheme(new TestEndpointDataSource(routeEndpoint, placeholder));

        // Act
        var endpoints = scheme.FindEndpoints("Api").ToList();

        // Assert
        Assert.Contains(routeEndpoint, endpoints);
        Assert.Single(endpoints.OfType<RouteEndpoint>());
    }

    [Fact]
    public void FindEndpoints_TwoRouteEndpointsShareName_Throws()
    {
        // Arrange
        var first = CreateRouteEndpoint("Api", "first");
        var second = CreateRouteEndpoint("Api", "second");
        var scheme = new ShellEndpointNameAddressScheme(new TestEndpointDataSource(first, second));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => scheme.FindEndpoints("Api").ToList());

        Assert.Contains("duplicate endpoint name", exception.Message);
        Assert.Contains("Api", exception.Message);
    }

    [Fact]
    public void FindEndpoints_SuppressedDuplicate_IsIgnored()
    {
        // Arrange
        var routeEndpoint = CreateRouteEndpoint("Api");
        var suppressed = CreateRouteEndpoint("Api", "suppressed", suppressLinkGeneration: true);
        var scheme = new ShellEndpointNameAddressScheme(new TestEndpointDataSource(routeEndpoint, suppressed));

        // Act
        var endpoints = scheme.FindEndpoints("Api").ToList();

        // Assert
        Assert.Single(endpoints);
        Assert.Same(routeEndpoint, endpoints[0]);
    }

    [Fact]
    public void FindEndpoints_UnknownName_ReturnsEmpty()
    {
        // Arrange
        var scheme = new ShellEndpointNameAddressScheme(new TestEndpointDataSource(CreateRouteEndpoint("Api")));

        // Act
        var endpoints = scheme.FindEndpoints("Missing");

        // Assert
        Assert.Empty(endpoints);
    }

    [Fact]
    public void FindEndpoints_RebuildsLookup_WhenChangeTokenFires()
    {
        // Arrange
        var dataSource = new TestEndpointDataSource(CreateRouteEndpoint("Api"));
        var scheme = new ShellEndpointNameAddressScheme(dataSource);

        Assert.Single(scheme.FindEndpoints("Api"));
        Assert.Empty(scheme.FindEndpoints("Added"));

        // Act
        dataSource.SetEndpoints(CreateRouteEndpoint("Api"), CreateRouteEndpoint("Added"));

        // Assert
        Assert.Single(scheme.FindEndpoints("Api"));
        Assert.Single(scheme.FindEndpoints("Added"));
    }

    private static RouteEndpoint CreateRouteEndpoint(string endpointName, string displayName = null, bool suppressLinkGeneration = false)
    {
        var metadata = new List<object>
        {
            new EndpointNameMetadata(endpointName),
        };

        if (suppressLinkGeneration)
        {
            metadata.Add(new SuppressLinkGenerationMetadata());
        }

        return new RouteEndpoint(
            context => Task.CompletedTask,
            RoutePatternFactory.Parse("{name}"),
            order: 0,
            new EndpointMetadataCollection(metadata),
            displayName ?? endpointName);
    }

    private static Endpoint CreatePlaceholderEndpoint(string endpointName)
    {
        return new Endpoint(
            requestDelegate: null,
            new EndpointMetadataCollection(new EndpointNameMetadata(endpointName)),
            displayName: endpointName + " (placeholder)");
    }

    private sealed class SuppressLinkGenerationMetadata : ISuppressLinkGenerationMetadata
    {
        public bool SuppressLinkGeneration => true;
    }

    private sealed class TestEndpointDataSource : EndpointDataSource
    {
        private List<Endpoint> _endpoints;
        private CancellationTokenSource _cts;
        private CancellationChangeToken _changeToken;

        public TestEndpointDataSource(params Endpoint[] endpoints)
        {
            _endpoints = [.. endpoints];
            _cts = new CancellationTokenSource();
            _changeToken = new CancellationChangeToken(_cts.Token);
        }

        public override IReadOnlyList<Endpoint> Endpoints => _endpoints;

        public override IChangeToken GetChangeToken() => _changeToken;

        public void SetEndpoints(params Endpoint[] endpoints)
        {
            var previous = _cts;

            _endpoints = [.. endpoints];
            _cts = new CancellationTokenSource();
            _changeToken = new CancellationChangeToken(_cts.Token);

            previous.Cancel();
            previous.Dispose();
        }
    }
}
