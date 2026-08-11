using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace OrchardCore.Infrastructure.Security;

/// <summary>
/// Authenticates authorized endpoint requests using the schemes configured by
/// <see cref="AuthorizeWithSchemesAttribute"/> so token-based clients, such as headless front-ends,
/// mobile applications, and service-to-service callers, can use additional schemes without replacing the
/// host's default authentication flow.
/// </summary>
/// <remarks>
/// Endpoints opt in by using <see cref="AuthorizeWithSchemesAttribute"/>, so unrelated routes are never affected.
/// The middleware preserves ASP.NET Core's default challenge flow by updating <see cref="HttpContext.User"/>
/// before authorization runs instead of rewriting the endpoint's authorization metadata. For SignalR, browsers
/// cannot send an <c>Authorization</c> header during a WebSocket handshake, so clients send bearer tokens using
/// the standard <c>access_token</c> query string parameter, which this middleware promotes to an
/// <c>Authorization</c> header before authenticating the configured schemes.
/// </remarks>
public sealed class AuthorizeWithSchemesMiddleware
{
    private const string BearerPrefix = "Bearer ";

    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public AuthorizeWithSchemesMiddleware(
        RequestDelegate next,
        ILogger<AuthorizeWithSchemesMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (TryGetAuthentication(context, out var authentication, out var endpointDisplayName))
        {
            if (!authentication.IncludeDefaultAuthenticateScheme &&
                context.User?.Identity?.IsAuthenticated == true)
            {
                context.User = new ClaimsPrincipal(new ClaimsIdentity());
            }

            if (context.User?.Identity?.IsAuthenticated != true)
            {
                await AuthenticateAsync(context, endpointDisplayName, authentication.AuthenticationSchemes);
            }
        }

        await _next(context);
    }

    internal static bool TryGetAuthentication(
        HttpContext context,
        out AuthorizeWithSchemesAttribute authentication,
        out string endpointDisplayName)
    {
        authentication = null;
        endpointDisplayName = null;

        var endpoint = context.GetEndpoint();

        if (endpoint is null)
        {
            return false;
        }

        var authorizeData = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();

        if (authorizeData.Count == 0)
        {
            return false;
        }

        authentication = endpoint.Metadata.GetMetadata<AuthorizeWithSchemesAttribute>();

        if (authentication is null)
        {
            return false;
        }

        endpointDisplayName = GetEndpointDisplayName(endpoint);

        return true;
    }

    private async Task AuthenticateAsync(HttpContext context, string endpointDisplayName, string authenticationSchemes)
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
                // The endpoint display name comes from the endpoint metadata rather than from the request, so nothing
                // a caller controls is ever written to the log.
                _logger.LogDebug(result.Failure, "Unable to authenticate a request for the '{Endpoint}' endpoint using the '{Scheme}' authentication scheme.", endpointDisplayName, scheme);
            }
        }
    }

    private static string GetEndpointDisplayName(Endpoint endpoint)
    {
        if (endpoint.Metadata.GetMetadata<HubMetadata>() is { HubType: { } hubType })
        {
            return hubType.FullName ?? hubType.Name;
        }

        return string.IsNullOrWhiteSpace(endpoint.DisplayName) ? "endpoint" : endpoint.DisplayName;
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
