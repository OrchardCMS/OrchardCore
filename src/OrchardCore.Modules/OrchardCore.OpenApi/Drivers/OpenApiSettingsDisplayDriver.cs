using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
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

    private readonly IEnumerable<IOpenApiUIFeature> _uiFeatures;
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpClientFactory _httpClientFactory;

    internal readonly IStringLocalizer S;

    public OpenApiSettingsDisplayDriver(
        IEnumerable<IOpenApiUIFeature> uiFeatures,
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IHttpClientFactory httpClientFactory,
        IStringLocalizer<OpenApiSettingsDisplayDriver> stringLocalizer)
    {
        _uiFeatures = uiFeatures;
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

        if (!await _authorizationService.AuthorizeAsync(user, OpenApiPermissions.ManageOpenApi))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<OpenApiSettingsViewModel>("OpenApiSettings_Edit", model =>
        {
            model.IsSwaggerUIEnabled = _uiFeatures.Any(f => f is SwaggerUIFeature);
            model.IsReDocUIEnabled = _uiFeatures.Any(f => f is ReDocUIFeature);
            model.IsScalarUIEnabled = _uiFeatures.Any(f => f is ScalarUIFeature);
            model.AllowAnonymousSchemaAccess = settings.AllowAnonymousSchemaAccess;
            model.AuthenticationType = settings.AuthenticationType;
            model.AuthorizationUrl = settings.AuthorizationUrl;
            model.TokenUrl = settings.TokenUrl;
            model.ServerMetadataUrl = settings.ServerMetadataUrl;
            model.OAuthClientId = settings.OAuthClientId;
            model.OAuthScopes = settings.OAuthScopes;
        }).Location("Content")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, OpenApiSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, OpenApiPermissions.ManageOpenApi))
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

        // When a server metadata URL is explicitly configured, validate the configuration
        // against that document. The metadata location is never inferred from the endpoint
        // URLs: the spec does not tie endpoint locations to the issuer, so any such inference
        // (e.g. assuming an OpenIddict-style "/connect/" path) breaks valid third-party setups.
        if (model.AuthenticationType != OpenApiAuthenticationType.None
            && context.Updater.ModelState.IsValid
            && !string.IsNullOrWhiteSpace(model.ServerMetadataUrl))
        {
            await ValidateServerMetadataAsync(model, context);
        }

        if (!context.Updater.ModelState.IsValid)
        {
            return await EditAsync(site, settings, context);
        }

        settings.AllowAnonymousSchemaAccess = model.AllowAnonymousSchemaAccess;
        settings.AuthenticationType = model.AuthenticationType;
        settings.AuthorizationUrl = model.AuthorizationUrl;
        settings.TokenUrl = model.TokenUrl;
        settings.ServerMetadataUrl = model.ServerMetadataUrl;
        settings.OAuthClientId = model.OAuthClientId;
        settings.OAuthScopes = model.OAuthScopes;

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }

    private async Task ValidateServerMetadataAsync(OpenApiSettingsViewModel model, UpdateEditorContext context)
    {
        var metadataUrl = ResolveUrl(model.ServerMetadataUrl);

        if (metadataUrl == null || !Uri.TryCreate(metadataUrl, UriKind.Absolute, out var metadataUri))
        {
            context.Updater.ModelState.AddModelError(Prefix, S["Could not resolve the Server Metadata URL."]);
            return;
        }

        if (!OpenApiUrlGuard.IsExternalUrlAllowed(metadataUri, out var blockedReason))
        {
            context.Updater.ModelState.AddModelError(Prefix, S[blockedReason]);
            return;
        }

        try
        {
            var client = _httpClientFactory.CreateClient(OpenApiUrlGuard.HttpClientName);

            // RequireHttps is relaxed because this is a validation-only fetch over the
            // SSRF-guarded client, and same-host tenants commonly serve metadata over
            // plain http in local setups.
            var retriever = new HttpDocumentRetriever(client)
            {
                RequireHttps = false,
            };

            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                metadataUri.AbsoluteUri,
                new OpenIdConnectConfigurationRetriever(),
                retriever);

            var configuration = await configurationManager.GetConfigurationAsync(CancellationToken.None);

            if (model.AuthenticationType == OpenApiAuthenticationType.AuthorizationCodePkce)
            {
                if (string.IsNullOrEmpty(configuration.AuthorizationEndpoint))
                {
                    context.Updater.ModelState.AddModelError(Prefix, S["The OpenID Connect server does not expose an authorization endpoint. Verify that the Authorization Code flow is enabled."]);
                }

                if (configuration.GrantTypesSupported.Count > 0
                    && !configuration.GrantTypesSupported.Contains("authorization_code"))
                {
                    context.Updater.ModelState.AddModelError(Prefix, S["The OpenID Connect server does not support the Authorization Code grant type. Enable it in the OpenID Connect server settings."]);
                }
            }

            if (string.IsNullOrEmpty(configuration.TokenEndpoint))
            {
                context.Updater.ModelState.AddModelError(Prefix, S["The OpenID Connect server does not expose a token endpoint. Verify the server configuration."]);
            }
        }
        catch (Exception exception) when (exception is InvalidOperationException or ArgumentException or IOException)
        {
            context.Updater.ModelState.AddModelError(Prefix, S["Could not retrieve valid OpenID Connect server metadata from \"{0}\". Verify that the server is running and the URL is correct.", metadataUri.AbsoluteUri]);
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
