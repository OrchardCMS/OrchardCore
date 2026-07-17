using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Modules;
using OrchardCore.OpenApi.Settings;

namespace OrchardCore.OpenApi.Endpoints.Api;

public static class TestConnectionEndpoint
{
    public static IEndpointRouteBuilder AddTestConnectionEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("api/openapi/test-connection", HandleAsync)
            .WithName("ApiTestOpenApiConnection")
            .WithTags("OpenApiApi")
            .AllowAnonymous()
            .AddEndpointFilter<RequireAntiforgeryForCookieAuthFilter>()
            .DisableAntiforgery()
            .Produces<TestConnectionResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return builder;
    }

    /// <summary>
    /// Tests the connection to an OpenID Connect server by validating the discovery document.
    /// </summary>
    [Authorize(AuthenticationSchemes = OpenApiAuthenticationDefaults.CookieOrTokenScheme)]
    private static async Task<IResult> HandleAsync(
        TestConnectionRequest model,
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IHttpClientFactory httpClientFactory,
        IStringLocalizer<TestConnectionEndpointResources> S)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, OpenApiPermissions.ManageOpenApi))
        {
            return httpContext.ChallengeOrForbid(OpenApiAuthenticationDefaults.CookieOrTokenScheme);
        }

        var modelState = new ModelStateDictionary();

        if (string.IsNullOrWhiteSpace(model.TokenUrl))
        {
            modelState.AddModelError(nameof(model.TokenUrl), S["Token URL is required."]);
        }

        if (string.IsNullOrWhiteSpace(model.ClientId))
        {
            modelState.AddModelError(nameof(model.ClientId), S["Client ID is required."]);
        }

        if (!modelState.IsValid)
        {
            return httpContext.ApiValidationProblem(modelState: modelState);
        }

        var tokenUrl = ResolveUrl(model.TokenUrl, httpContext);

        if (tokenUrl == null)
        {
            return httpContext.ApiBadRequestProblem(detail: S["Could not resolve the Token URL."]);
        }

        if (!OpenApiUrlGuard.IsExternalUrlAllowed(new Uri(tokenUrl, UriKind.Absolute), out var blockedReason))
        {
            return httpContext.ApiBadRequestProblem(detail: S[blockedReason]);
        }

        // Validate the OpenID Connect discovery document.
        var discoveryError = await ValidateDiscoveryAsync(tokenUrl, model.AuthenticationType, httpClientFactory, S);

        if (discoveryError != null)
        {
            return httpContext.ApiBadRequestProblem(detail: discoveryError);
        }

        // For PKCE, we can only validate the discovery document since PKCE requires a browser redirect.
        if (model.AuthenticationType == OpenApiAuthenticationType.AuthorizationCodePkce)
        {
            return Results.Ok(new TestConnectionResult
            {
                Message = S["OpenID Connect server validated successfully. Authorization Code + PKCE requires a browser redirect and cannot be fully tested here."],
            });
        }

        return Results.Ok(new TestConnectionResult
        {
            Message = S["Connection validated successfully."],
        });
    }

    /// <summary>
    /// Validates the OpenID Connect discovery document.
    /// Returns null on success, or a localized error message on failure.
    /// </summary>
    private static async Task<LocalizedString> ValidateDiscoveryAsync(
        string tokenUrl,
        OpenApiAuthenticationType authType,
        IHttpClientFactory httpClientFactory,
        IStringLocalizer S
    )
    {
        var tokenUri = new Uri(tokenUrl, UriKind.Absolute);
        var issuerBase = $"{tokenUri.Scheme}://{tokenUri.Authority}";

        var tokenPath = tokenUri.AbsolutePath;
        var connectIndex = tokenPath.IndexOf("/connect/", StringComparison.OrdinalIgnoreCase);

        if (connectIndex > 0)
        {
            issuerBase += tokenPath[..connectIndex];
        }

        var discoveryUrl = $"{issuerBase}/.well-known/openid-configuration";

        try
        {
            var client = httpClientFactory.CreateClient(OpenApiUrlGuard.HttpClientName);
            var response = await client.GetAsync(discoveryUrl);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return S["Could not find the OpenID Connect discovery document at \"{0}\". Verify that the OpenID Connect server is enabled.", discoveryUrl];
            }

            if (!response.IsSuccessStatusCode)
            {
                return S["The discovery document at \"{0}\" returned status {1}.", discoveryUrl, (int)response.StatusCode];
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (authType == OpenApiAuthenticationType.AuthorizationCodePkce)
            {
                if (!root.TryGetProperty("authorization_endpoint", out _))
                {
                    return S["The OpenID Connect server does not expose an authorization endpoint."];
                }

                if (root.TryGetProperty("grant_types_supported", out var grantTypes))
                {
                    var hasAuthCode = false;

                    foreach (var gt in grantTypes.EnumerateArray())
                    {
                        if (gt.GetString() == "authorization_code")
                        {
                            hasAuthCode = true;
                            break;
                        }
                    }

                    if (!hasAuthCode)
                    {
                        return S["The OpenID Connect server does not support the Authorization Code grant type."];
                    }
                }
            }

            if (!root.TryGetProperty("token_endpoint", out _))
            {
                return S["The OpenID Connect server does not expose a token endpoint."];
            }

            return null;
        }
        catch (HttpRequestException)
        {
            return S["Could not reach the OpenID Connect server. Verify that the server is running and the URL is correct."];
        }
        catch (JsonException)
        {
            return S["The OpenID Connect discovery document could not be parsed."];
        }
    }

    private static string ResolveUrl(string url, HttpContext httpContext)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return url;
        }

        var request = httpContext?.Request;

        if (request == null)
        {
            return null;
        }

        return $"{request.Scheme}://{request.Host}{request.PathBase}{url}";
    }
}

/// <summary>
/// Pure marker type for <see cref="IStringLocalizer{T}"/> resource resolution — this endpoint's
/// handler is a static method, which cannot itself be used as a generic type argument.
/// </summary>
internal sealed class TestConnectionEndpointResources;

internal sealed class TestConnectionRequest
{
    public OpenApiAuthenticationType AuthenticationType { get; set; }
    public string TokenUrl { get; set; }
    public string AuthorizationUrl { get; set; }
    public string ClientId { get; set; }
    public string Scopes { get; set; }
}

internal sealed class TestConnectionResult
{
    public string Message { get; set; }
}
