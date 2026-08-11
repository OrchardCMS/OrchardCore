using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.SignalR.Services;

/// <summary>
/// Promotes the SignalR <c>access_token</c> query string parameter to an
/// <c>Authorization</c> header so bearer authentication handlers can authenticate hub
/// requests during the WebSocket handshake.
/// </summary>
public sealed class AccessTokenHeaderMiddleware
{
    private const string s_bearerPrefix = "Bearer ";

    private readonly RequestDelegate _next;

    public AccessTokenHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.GetEndpoint()?.Metadata.GetMetadata<HubMetadata>() is not null &&
            StringValues.IsNullOrEmpty(context.Request.Headers.Authorization) &&
            !StringValues.IsNullOrEmpty(context.Request.Query[OrchardCoreConstants.TokenNames.AccessToken]))
        {
            context.Request.Headers.Authorization =
                s_bearerPrefix + context.Request.Query[OrchardCoreConstants.TokenNames.AccessToken];
        }

        return _next(context);
    }
}
