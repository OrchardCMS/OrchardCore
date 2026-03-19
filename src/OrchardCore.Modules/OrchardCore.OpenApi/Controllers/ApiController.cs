using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.OpenApi.Settings;

namespace OrchardCore.OpenApi.Controllers;

[ApiController]
[Route("api/openapi")]
[Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
public sealed class OpenApiApiController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public OpenApiApiController(
        IAuthorizationService authorizationService,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<OpenApiApiController> stringLocalizer
    )
    {
        _authorizationService = authorizationService;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        S = stringLocalizer;
    }

    /// <summary>
    /// Tests the connection to an OpenID Connect server by validating the discovery
    /// document and optionally performing a token exchange for Client Credentials.
    /// </summary>
    [HttpPost("test-connection")]
    [ProducesResponseType(typeof(TestConnectionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> TestConnection([FromBody] TestConnectionRequest model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, OpenApiPermissions.ApiManage))
        {
            return this.ChallengeOrForbid("Api");
        }

        if (string.IsNullOrWhiteSpace(model.TokenUrl))
        {
            ModelState.AddModelError(nameof(model.TokenUrl), S["Token URL is required."]);
        }

        if (string.IsNullOrWhiteSpace(model.ClientId))
        {
            ModelState.AddModelError(nameof(model.ClientId), S["Client ID is required."]);
        }

        if (!ModelState.IsValid)
        {
            return this.ApiValidationProblem(modelState: ModelState);
        }

        var tokenUrl = ResolveUrl(model.TokenUrl);

        if (tokenUrl == null)
        {
            return this.ApiBadRequestProblem(detail: S["Could not resolve the Token URL."]);
        }

        // Step 1: Validate the OpenID Connect discovery document.
        var discoveryError = await ValidateDiscoveryAsync(tokenUrl, model.AuthenticationType);

        if (discoveryError != null)
        {
            return this.ApiBadRequestProblem(detail: discoveryError);
        }

        // Step 2: For Client Credentials, perform a real token exchange.
        if (model.AuthenticationType == OpenApiAuthenticationType.ClientCredentials)
        {
            if (string.IsNullOrWhiteSpace(model.ClientSecret))
            {
                ModelState.AddModelError(
                    nameof(model.ClientSecret),
                    S["Client Secret is required for Client Credentials flow testing."]
                );

                return this.ApiValidationProblem(modelState: ModelState);
            }

            var tokenResult = await ExchangeClientCredentialsAsync(tokenUrl, model);

            if (!tokenResult.Success)
            {
                return this.ApiBadRequestProblem(detail: tokenResult.Error);
            }

            // Step 3: Verify the token works by calling the swagger endpoint.
            var verifyError = await VerifyTokenAsync(tokenResult.Token);

            if (verifyError != null)
            {
                return this.ApiBadRequestProblem(detail: verifyError);
            }

            return Ok(new TestConnectionResult
            {
                Message = S["Connection successful! Token exchange and API endpoint verification passed."],
            });
        }

        // For PKCE, we can only validate the discovery document since PKCE requires a browser redirect.
        if (model.AuthenticationType == OpenApiAuthenticationType.AuthorizationCodePkce)
        {
            return Ok(new TestConnectionResult
            {
                Message = S["OpenID Connect server validated successfully. Authorization Code + PKCE requires a browser redirect and cannot be fully tested here."],
            });
        }

        return Ok(new TestConnectionResult
        {
            Message = S["Connection validated successfully."],
        });
    }

    /// <summary>
    /// Validates the OpenID Connect discovery document.
    /// Returns null on success, or a localized error message on failure.
    /// </summary>
    private async Task<LocalizedString> ValidateDiscoveryAsync(
        string tokenUrl,
        OpenApiAuthenticationType authType
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
            var client = _httpClientFactory.CreateClient();
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
            else if (authType == OpenApiAuthenticationType.ClientCredentials)
            {
                if (root.TryGetProperty("grant_types_supported", out var grantTypes))
                {
                    var hasClientCredentials = false;

                    foreach (var gt in grantTypes.EnumerateArray())
                    {
                        if (gt.GetString() == "client_credentials")
                        {
                            hasClientCredentials = true;
                            break;
                        }
                    }

                    if (!hasClientCredentials)
                    {
                        return S["The OpenID Connect server does not support the Client Credentials grant type."];
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
    }

    /// <summary>
    /// Exchanges client credentials for an access token.
    /// Returns a result with the token on success, or a localized error on failure.
    /// </summary>
    private async Task<TokenExchangeResult> ExchangeClientCredentialsAsync(
        string tokenUrl,
        TestConnectionRequest model
    )
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = model.ClientId,
                ["client_secret"] = model.ClientSecret,
            };

            if (!string.IsNullOrWhiteSpace(model.Scopes))
            {
                parameters["scope"] = model.Scopes;
            }

            var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync(tokenUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                LocalizedString errorMessage = S["Token request failed with status {0}.", (int)response.StatusCode];

                try
                {
                    var errorDoc = JsonDocument.Parse(body);

                    if (errorDoc.RootElement.TryGetProperty("error_description", out var desc))
                    {
                        errorMessage = S["Token request failed: {0}", desc.GetString()];
                    }
                    else if (errorDoc.RootElement.TryGetProperty("error", out var error))
                    {
                        errorMessage = S["Token request failed: {0}", error.GetString()];
                    }
                }
                catch (JsonException)
                {
                    // Use the default error message.
                }

                return TokenExchangeResult.Failure(errorMessage);
            }

            var tokenJson = await response.Content.ReadAsStringAsync();
            var tokenDoc = JsonDocument.Parse(tokenJson);

            if (!tokenDoc.RootElement.TryGetProperty("access_token", out var accessToken))
            {
                return TokenExchangeResult.Failure(S["Token response did not contain an access_token."]);
            }

            return TokenExchangeResult.Ok(accessToken.GetString());
        }
        catch (HttpRequestException ex)
        {
            return TokenExchangeResult.Failure(S["Could not reach the token endpoint: {0}", ex.Message]);
        }
    }

    /// <summary>
    /// Verifies a Bearer token by calling the Swagger endpoint.
    /// Returns null on success, or a localized error message on failure.
    /// </summary>
    private async Task<LocalizedString> VerifyTokenAsync(string token)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var swaggerUrl = ResolveUrl("/swagger/v1/swagger.json");

            if (swaggerUrl == null)
            {
                // Not a failure — we just can't verify.
                return null;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, swaggerUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return null;
            }

            return S["Token was obtained but the API endpoint returned status {0}. Verify the client has the required permissions.", (int)response.StatusCode];
        }
        catch (HttpRequestException ex)
        {
            return S["Token exchange successful, but could not verify the API endpoint: {0}", ex.Message];
        }
    }

    private string ResolveUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return url;
        }

        var request = _httpContextAccessor.HttpContext?.Request;

        if (request == null)
        {
            return null;
        }

        return $"{request.Scheme}://{request.Host}{request.PathBase}{url}";
    }
}

public sealed class TestConnectionRequest
{
    public OpenApiAuthenticationType AuthenticationType { get; set; }
    public string TokenUrl { get; set; }
    public string AuthorizationUrl { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Scopes { get; set; }
}

public sealed class TestConnectionResult
{
    public string Message { get; set; }
}

internal sealed class TokenExchangeResult
{
    public bool Success { get; set; }
    public string Token { get; set; }
    public LocalizedString Error { get; set; }

    public static TokenExchangeResult Ok(string token) => new() { Success = true, Token = token };
    public static TokenExchangeResult Failure(LocalizedString error) => new() { Error = error };
}
