using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenApi.Settings;
using OrchardCore.OpenApi.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.OpenApi.Drivers;

public sealed class OpenApiSettingsDisplayDriver : SiteDisplayDriver<OpenApiSettings>
{
    public const string GroupId = "openapi";

    private readonly IShellFeaturesManager _shellFeaturesManager;
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpClientFactory _httpClientFactory;

    internal readonly IStringLocalizer S;

    public OpenApiSettingsDisplayDriver(
        IShellFeaturesManager shellFeaturesManager,
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IHttpClientFactory httpClientFactory,
        IStringLocalizer<OpenApiSettingsDisplayDriver> stringLocalizer)
    {
        _shellFeaturesManager = shellFeaturesManager;
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _httpClientFactory = httpClientFactory;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, OpenApiSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, OpenApiPermissions.ApiManage))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        var enabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();
        var enabledFeatureIds = enabledFeatures.Select(f => f.Id).ToHashSet();

        return Initialize<OpenApiSettingsViewModel>("OpenApiSettings_Edit", model =>
        {
            model.IsSwaggerUIEnabled = enabledFeatureIds.Contains("OrchardCore.OpenApi.SwaggerUI");
            model.IsReDocUIEnabled = enabledFeatureIds.Contains("OrchardCore.OpenApi.ReDocUI");
            model.IsScalarUIEnabled = enabledFeatureIds.Contains("OrchardCore.OpenApi.ScalarUI");
            model.AuthenticationType = settings.AuthenticationType;
            model.AuthorizationUrl = settings.AuthorizationUrl;
            model.TokenUrl = settings.TokenUrl;
            model.OAuthClientId = settings.OAuthClientId;
            model.OAuthScopes = settings.OAuthScopes;
        }).Location("Content")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, OpenApiSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, OpenApiPermissions.ApiManage))
        {
            return null;
        }

        var model = new OpenApiSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (model.AuthenticationType == OpenApiAuthenticationType.AuthorizationCodePkce)
        {
            if (string.IsNullOrWhiteSpace(model.AuthorizationUrl))
            {
                context.Updater.ModelState.AddModelError(Prefix, S["The Authorization URL is required for Authorization Code + PKCE."]);
            }

            if (string.IsNullOrWhiteSpace(model.TokenUrl))
            {
                context.Updater.ModelState.AddModelError(Prefix, S["The Token URL is required for Authorization Code + PKCE."]);
            }

            if (string.IsNullOrWhiteSpace(model.OAuthClientId))
            {
                context.Updater.ModelState.AddModelError(Prefix, S["The Client ID is required for Authorization Code + PKCE."]);
            }
        }
        else if (model.AuthenticationType == OpenApiAuthenticationType.ClientCredentials)
        {
            if (string.IsNullOrWhiteSpace(model.TokenUrl))
            {
                context.Updater.ModelState.AddModelError(Prefix, S["The Token URL is required for Client Credentials."]);
            }
        }

        // Validate the OAuth2 server by fetching its OpenID Connect discovery document.
        if (model.AuthenticationType != OpenApiAuthenticationType.None
            && context.Updater.ModelState.IsValid
            && !string.IsNullOrWhiteSpace(model.TokenUrl))
        {
            await ValidateOAuthServerAsync(model, context);
        }

        if (!context.Updater.ModelState.IsValid)
        {
            return await EditAsync(site, settings, context);
        }

        settings.AuthenticationType = model.AuthenticationType;
        settings.AuthorizationUrl = model.AuthorizationUrl;
        settings.TokenUrl = model.TokenUrl;
        settings.OAuthClientId = model.OAuthClientId;
        settings.OAuthScopes = model.OAuthScopes;

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }

    private async Task ValidateOAuthServerAsync(OpenApiSettingsViewModel model, UpdateEditorContext context)
    {
        // Derive the issuer base URL from the token URL to find the discovery document.
        // e.g., "/connect/token" → base is the tenant root; "https://auth.example.com/connect/token" → base is "https://auth.example.com".
        var tokenUrl = ResolveUrl(model.TokenUrl);

        if (tokenUrl == null)
        {
            return;
        }

        var tokenUri = new Uri(tokenUrl, UriKind.Absolute);
        var issuerBase = $"{tokenUri.Scheme}://{tokenUri.Authority}";

        // Also include the path prefix before /connect/ if present (e.g., tenant prefix).
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
                context.Updater.ModelState.AddModelError(Prefix, S["Could not find the OpenID Connect discovery document at \"{0}\". Verify that the OpenID Connect server is enabled.", discoveryUrl]);
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                context.Updater.ModelState.AddModelError(Prefix, S["The OpenID Connect discovery document at \"{0}\" returned status {1}.", discoveryUrl, (int)response.StatusCode]);
                return;
            }

            // Parse the discovery document to verify the configured endpoints exist.
            var json = await response.Content.ReadAsStringAsync();
            var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (model.AuthenticationType == OpenApiAuthenticationType.AuthorizationCodePkce)
            {
                if (!root.TryGetProperty("authorization_endpoint", out _))
                {
                    context.Updater.ModelState.AddModelError(Prefix, S["The OpenID Connect server does not expose an authorization endpoint. Verify that the Authorization Code flow is enabled."]);
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
                        context.Updater.ModelState.AddModelError(Prefix, S["The OpenID Connect server does not support the Authorization Code grant type. Enable it in the OpenID Connect server settings."]);
                    }
                }
            }
            else if (model.AuthenticationType == OpenApiAuthenticationType.ClientCredentials)
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
                        context.Updater.ModelState.AddModelError(Prefix, S["The OpenID Connect server does not support the Client Credentials grant type. Enable it in the OpenID Connect server settings."]);
                    }
                }
            }

            if (!root.TryGetProperty("token_endpoint", out _))
            {
                context.Updater.ModelState.AddModelError(Prefix, S["The OpenID Connect server does not expose a token endpoint. Verify the server configuration."]);
            }
        }
        catch (HttpRequestException)
        {
            context.Updater.ModelState.AddModelError(Prefix, S["Could not reach the OpenID Connect server at \"{0}\". Verify that the server is running and the URL is correct.", issuerBase]);
        }
    }

    private string ResolveUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        // If the URL is already absolute, use it as-is.
        if (Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return url;
        }

        // Resolve relative URLs against the current request (includes tenant prefix via PathBase).
        var request = _httpContextAccessor.HttpContext?.Request;

        if (request == null)
        {
            return null;
        }

        return $"{request.Scheme}://{request.Host}{request.PathBase}{url}";
    }
}
