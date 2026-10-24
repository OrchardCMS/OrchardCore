using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes;

/// <summary>
/// This recipe step sets general OpenID Connect settings.
/// </summary>
public sealed class OpenIdServerSettingsStep : NamedRecipeStepHandler
{
    private readonly IOpenIdServerService _serverService;

    public OpenIdServerSettingsStep(IOpenIdServerService serverService)
        : base(nameof(OpenIdServerSettings))
    {
        _serverService = serverService;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<OpenIdServerSettingsStepModel>();
        var settings = await _serverService.LoadSettingsAsync();

        settings.AccessTokenFormat = model.OpenIdServerSettings.AccessTokenFormat;
        settings.Authority = !string.IsNullOrEmpty(model.OpenIdServerSettings.Authority) ? new Uri(model.OpenIdServerSettings.Authority, UriKind.Absolute) : null;

        settings.EncryptionCertificateStoreLocation = model.OpenIdServerSettings.EncryptionCertificateStoreLocation;
        settings.EncryptionCertificateStoreName = model.OpenIdServerSettings.EncryptionCertificateStoreName;
        settings.EncryptionCertificateThumbprint = model.OpenIdServerSettings.EncryptionCertificateThumbprint;

        settings.SigningCertificateStoreLocation = model.OpenIdServerSettings.SigningCertificateStoreLocation;
        settings.SigningCertificateStoreName = model.OpenIdServerSettings.SigningCertificateStoreName;
        settings.SigningCertificateThumbprint = model.OpenIdServerSettings.SigningCertificateThumbprint;

        settings.AuthorizationEndpointPath = model.OpenIdServerSettings.EnableAuthorizationEndpoint ?
            new PathString("/connect/authorize") : PathString.Empty;
        settings.LogoutEndpointPath = model.OpenIdServerSettings.EnableLogoutEndpoint ?
            new PathString("/connect/logout") : PathString.Empty;
        settings.TokenEndpointPath = model.OpenIdServerSettings.EnableTokenEndpoint ?
            new PathString("/connect/token") : PathString.Empty;
        settings.UserinfoEndpointPath = model.OpenIdServerSettings.EnableUserInfoEndpoint ?
            new PathString("/connect/userinfo") : PathString.Empty;
        settings.IntrospectionEndpointPath = model.OpenIdServerSettings.EnableIntrospectionEndpoint ?
            new PathString("/connect/introspect") : PathString.Empty;
        settings.PushedAuthorizationEndpointPath = model.OpenIdServerSettings.EnablePushedAuthorizationEndpoint ?
            new PathString("/connect/par") : PathString.Empty;
        settings.RevocationEndpointPath = model.OpenIdServerSettings.EnableRevocationEndpoint ?
            new PathString("/connect/revoke") : PathString.Empty;

        settings.AllowAuthorizationCodeFlow = model.OpenIdServerSettings.AllowAuthorizationCodeFlow;
        settings.AllowClientCredentialsFlow = model.OpenIdServerSettings.AllowClientCredentialsFlow;
        settings.AllowHybridFlow = model.OpenIdServerSettings.AllowHybridFlow;
        settings.AllowImplicitFlow = model.OpenIdServerSettings.AllowImplicitFlow;
        settings.AllowPasswordFlow = model.OpenIdServerSettings.AllowPasswordFlow;
        settings.AllowRefreshTokenFlow = model.OpenIdServerSettings.AllowRefreshTokenFlow;

        settings.DisableAccessTokenEncryption = model.OpenIdServerSettings.DisableAccessTokenEncryption;
        settings.DisableRollingRefreshTokens = model.OpenIdServerSettings.DisableRollingRefreshTokens;
        settings.UseReferenceAccessTokens = model.OpenIdServerSettings.UseReferenceAccessTokens;
        settings.RequireProofKeyForCodeExchange = model.OpenIdServerSettings.RequireProofKeyForCodeExchange;
        settings.RequirePushedAuthorizationRequests = model.OpenIdServerSettings.RequirePushedAuthorizationRequests;
        settings.RequireEndSessionConfirmation = model.OpenIdServerSettings.RequireEndSessionConfirmation;

        await _serverService.UpdateSettingsAsync(settings);
    }
}
