using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Net.Http.Headers;
using OrchardCore.SignalR.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.SignalR;

public sealed class AccessTokenHeaderMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithHubAccessTokenQuery_PromotesAuthorizationHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?access_token=test-token");
        context.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new HubMetadata(typeof(TestHub))),
            "Test hub"));

        await new AccessTokenHeaderMiddleware(_ => Task.CompletedTask).InvokeAsync(context);

        Assert.Equal("Bearer test-token", context.Request.Headers.Authorization.ToString());
    }

    [Fact]
    public async Task InvokeAsync_WithExistingAuthorizationHeader_LeavesHeaderUntouched()
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?access_token=test-token");
        context.Request.Headers.Authorization = "Bearer existing-token";
        context.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new HubMetadata(typeof(TestHub))),
            "Test hub"));

        await new AccessTokenHeaderMiddleware(_ => Task.CompletedTask).InvokeAsync(context);

        Assert.Equal("Bearer existing-token", context.Request.Headers.Authorization.ToString());
    }

    [Fact]
    public async Task InvokeAsync_WithoutHubMetadata_DoesNotPromoteAuthorizationHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?access_token=test-token");
        context.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(),
            "Test endpoint"));

        await new AccessTokenHeaderMiddleware(_ => Task.CompletedTask).InvokeAsync(context);

        Assert.False(context.Request.Headers.ContainsKey(HeaderNames.Authorization));
    }

    private sealed class TestHub : Hub;
}
