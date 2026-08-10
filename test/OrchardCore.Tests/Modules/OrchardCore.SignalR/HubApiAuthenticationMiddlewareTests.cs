using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.SignalR.Middlewares;

namespace OrchardCore.Tests.Modules.OrchardCore.SignalR;

public sealed class HubApiAuthenticationMiddlewareTests
{
    [Fact]
    public async Task GetAnonymousHubUsingApiAuthenticationAsync_ApiPolicy_ReturnsHubType()
    {
        // Arrange
        var context = CreateContext("Api");

        // Act
        var hubType = await HubApiAuthenticationMiddleware.GetAnonymousHubUsingApiAuthenticationAsync(context);

        // Assert
        Assert.Equal(typeof(TestHub), hubType);
    }

    [Fact]
    public async Task GetAnonymousHubUsingApiAuthenticationAsync_CookiePolicy_ReturnsNull()
    {
        // Arrange
        var context = CreateContext("Identity.Application");

        // Act
        var hubType = await HubApiAuthenticationMiddleware.GetAnonymousHubUsingApiAuthenticationAsync(context);

        // Assert
        Assert.Null(hubType);
    }

    private static DefaultHttpContext CreateContext(string authenticationScheme)
    {
        var services = new ServiceCollection();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("HubPolicy", policy =>
            {
                policy.AddAuthenticationSchemes(authenticationScheme);
                policy.RequireAuthenticatedUser();
            });
        });

        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider(),
        };

        context.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(
                new HubMetadata(typeof(TestHub)),
                new AuthorizeAttribute("HubPolicy")),
            "Test hub"));

        return context;
    }

    private sealed class TestHub : Hub;
}
