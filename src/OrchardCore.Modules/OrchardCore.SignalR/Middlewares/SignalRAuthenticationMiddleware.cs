using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace OrchardCore.SignalR.Middlewares;

/// <summary>
/// Authenticates SignalR hub requests using the schemes configured by
/// <see cref="AuthorizeSignalRAttribute"/> so token based clients, such as headless front-ends,
/// mobile applications, and service-to-service callers, can connect to hubs the same way they call the
/// API endpoints.
/// </summary>
/// <remarks>
/// Hubs opt in by using <see cref="AuthorizeSignalRAttribute"/>, so other hubs are never affected.
/// The middleware preserves ASP.NET Core's default challenge flow by updating <see cref="HttpContext.User"/>
/// before authorization runs instead of rewriting the hub's authorization metadata. Browsers cannot send an
/// <c>Authorization</c> header during a WebSocket handshake, so SignalR clients send bearer tokens using the
/// standard <c>access_token</c> query string parameter, which this middleware promotes to an
/// <c>Authorization</c> header before authenticating the configured schemes.
/// </remarks>
public sealed class SignalRAuthenticationMiddleware
{
    private const string BearerPrefix = "Bearer ";

    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public SignalRAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<SignalRAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (TryGetHubAuthentication(context, out var authentication, out var hubType))
        {
            if (!authentication.IncludeDefaultAuthenticateScheme &&
                context.User?.Identity?.IsAuthenticated == true)
            {
                context.User = new ClaimsPrincipal(new ClaimsIdentity());
            }

            if (context.User?.Identity?.IsAuthenticated != true)
            {
                await AuthenticateAsync(context, hubType, authentication.AuthenticationSchemes);
            }
        }

        await _next(context);
    }

    internal static bool TryGetHubAuthentication(
        HttpContext context,
        out AuthorizeSignalRAttribute authentication,
        out Type hubType)
    {
        authentication = null;
        hubType = null;

        var endpoint = context.GetEndpoint();
        var hubMetadata = endpoint?.Metadata.GetMetadata<HubMetadata>();

        if (hubMetadata is null)
        {
            return false;
        }

        var authorizeData = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();

        if (authorizeData.Count == 0)
        {
            return false;
        }

        authentication = endpoint.Metadata.GetMetadata<AuthorizeSignalRAttribute>();

        if (authentication is null)
        {
            return false;
        }

        hubType = hubMetadata.HubType;

        return true;
    }

    private async Task AuthenticateAsync(HttpContext context, Type hubType, string authenticationSchemes)
    {
        var schemes = ParseAuthenticationSchemes(authenticationSchemes);

        if (schemes.Count == 0)
        {
            return;
        }

        var accessToken = GetAccessToken(context.Request);

        if (string.IsNullOrEmpty(accessToken))
        {
            return;
        }

        var schemeProvider = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();

        context.Request.Headers.Authorization = BearerPrefix + accessToken;

        foreach (var scheme in schemes)
        {
            if (await schemeProvider.GetSchemeAsync(scheme) is null)
            {
                continue;
            }

            var result = await context.AuthenticateAsync(scheme);

            if (result.Succeeded && result.Principal is not null)
            {
                context.User = result.Principal;

                return;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                // The hub type comes from the endpoint metadata rather than from the request, so nothing
                // a caller controls is ever written to the log.
                _logger.LogDebug(result.Failure, "Unable to authenticate a request for the '{Hub}' hub using the '{Scheme}' authentication scheme.", hubType, scheme);
            }
        }
    }

    private static List<string> ParseAuthenticationSchemes(string authenticationSchemes)
    {
        if (string.IsNullOrWhiteSpace(authenticationSchemes))
        {
            return [];
        }

        return authenticationSchemes
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .ToList();
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

        return request.Query[OrchardCoreConstants.TokenNames.AccessToken].ToString();
    }
}
