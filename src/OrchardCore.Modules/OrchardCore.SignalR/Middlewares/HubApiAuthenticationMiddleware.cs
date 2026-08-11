using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace OrchardCore.SignalR.Middlewares;

/// <summary>
/// Authenticates SignalR hub requests using the OrchardCore <c>Api</c> authentication scheme so that
/// token based clients, such as headless front-ends, mobile applications, and service-to-service callers,
/// can connect to hubs the same way they call the API endpoints.
/// </summary>
/// <remarks>
/// Hubs opt in by using an authorization policy that includes the <c>Api</c> authentication scheme, so other hubs
/// are never affected. Cookie authenticated requests are left untouched. The <c>Api</c> scheme is only evaluated
/// when the request targets an opted-in hub endpoint, the caller is still anonymous, and a bearer token was provided.
/// Browsers cannot send an <c>Authorization</c> header during a WebSocket handshake, so SignalR clients send the
/// token using the standard <c>access_token</c> query string parameter, which this middleware promotes to an
/// <c>Authorization</c> header before authenticating.
/// </remarks>
public sealed class HubApiAuthenticationMiddleware
{
    private const string ApiAuthenticationScheme = "Api";

    private const string AccessTokenQueryParameterName = "access_token";

    private const string BearerPrefix = "Bearer ";

    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public HubApiAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<HubApiAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (await GetAnonymousHubUsingApiAuthenticationAsync(context) is { } hubType)
        {
            await AuthenticateAsync(context, hubType);
        }

        await _next(context);
    }

    internal static async ValueTask<Type> GetAnonymousHubUsingApiAuthenticationAsync(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            return null;
        }

        var endpoint = context.GetEndpoint();
        var hubMetadata = endpoint?.Metadata.GetMetadata<HubMetadata>();

        if (hubMetadata is null)
        {
            return null;
        }

        var authorizeData = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();

        if (authorizeData.Count == 0)
        {
            return null;
        }

        var policyProvider = context.RequestServices.GetRequiredService<IAuthorizationPolicyProvider>();
        var policy = await AuthorizationPolicy.CombineAsync(policyProvider, authorizeData);

        return policy?.AuthenticationSchemes.Contains(ApiAuthenticationScheme, StringComparer.Ordinal) == true
            ? hubMetadata.HubType
            : null;
    }

    private async Task AuthenticateAsync(HttpContext context, Type hubType)
    {
        var accessToken = GetAccessToken(context.Request);

        if (string.IsNullOrEmpty(accessToken))
        {
            return;
        }

        var schemeProvider = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();

        if (await schemeProvider.GetSchemeAsync(ApiAuthenticationScheme) is null)
        {
            return;
        }

        context.Request.Headers.Authorization = BearerPrefix + accessToken;

        var result = await context.AuthenticateAsync(ApiAuthenticationScheme);

        if (result.Succeeded && result.Principal is not null)
        {
            context.User = result.Principal;

            return;
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            // The hub type comes from the endpoint metadata rather than from the request, so nothing
            // a caller controls is ever written to the log.
            _logger.LogDebug(result.Failure, "Unable to authenticate a request for the '{Hub}' hub using the '{Scheme}' authentication scheme.", hubType, ApiAuthenticationScheme);
        }
    }

    private static string GetAccessToken(HttpRequest request)
    {
        var authorization = request.Headers[HeaderNames.Authorization].ToString();

        if (authorization.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return authorization[BearerPrefix.Length..].Trim();
        }

        if (!string.IsNullOrEmpty(authorization))
        {
            // A different authentication scheme is already in use. Leave the request untouched.
            return null;
        }

        return request.Query[AccessTokenQueryParameterName].ToString();
    }
}
