using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.OpenId.Recipes;
using OrchardCore.OpenId.Services;

namespace OrchardCore.OpenId.Deployment;

public class OpenIdServerDeploymentSource
    : DeploymentSourceBase<OpenIdServerDeploymentStep>
{
    private readonly IOpenIdServerService _openIdServerService;

    public OpenIdServerDeploymentSource(IOpenIdServerService openIdServerService)
    {
        _openIdServerService = openIdServerService;
    }

    protected override async Task ProcessAsync(OpenIdServerDeploymentStep step, DeploymentPlanResult result)
    {
        var settings = await _openIdServerService.GetSettingsAsync();

        var settingsModel = new OpenIdServerSettingsStepModel
        {
            AccessTokenFormat = settings.AccessTokenFormat,
            Authority = settings.Authority?.AbsoluteUri,

            EncryptionCertificateStoreLocation = settings.EncryptionCertificateStoreLocation,
            EncryptionCertificateStoreName = settings.EncryptionCertificateStoreName,
            EncryptionCertificateThumbprint = settings.EncryptionCertificateThumbprint,

            SigningCertificateStoreLocation = settings.SigningCertificateStoreLocation,
            SigningCertificateStoreName = settings.SigningCertificateStoreName,
            SigningCertificateThumbprint = settings.SigningCertificateThumbprint,

            // The recipe step only reads these flags, and uses constants for the paths.
            // Conversely, we export true for endpoints with a path, false for those without.
            EnableAuthorizationEndpoint = !string.IsNullOrWhiteSpace(settings.AuthorizationEndpointPath),
            EnableLogoutEndpoint = !string.IsNullOrWhiteSpace(settings.LogoutEndpointPath),
            EnableTokenEndpoint = !string.IsNullOrWhiteSpace(settings.TokenEndpointPath),
            EnableUserInfoEndpoint = !string.IsNullOrWhiteSpace(settings.UserinfoEndpointPath),
            EnableIntrospectionEndpoint = !string.IsNullOrWhiteSpace(settings.IntrospectionEndpointPath),
            EnableRevocationEndpoint = !string.IsNullOrWhiteSpace(settings.RevocationEndpointPath),

            AllowAuthorizationCodeFlow = settings.AllowAuthorizationCodeFlow,
            AllowClientCredentialsFlow = settings.AllowClientCredentialsFlow,
            AllowHybridFlow = settings.AllowHybridFlow,
            AllowImplicitFlow = settings.AllowImplicitFlow,
            AllowPasswordFlow = settings.AllowPasswordFlow,
            AllowRefreshTokenFlow = settings.AllowRefreshTokenFlow,

            DisableAccessTokenEncryption = settings.DisableAccessTokenEncryption,
            DisableRollingRefreshTokens = settings.DisableRollingRefreshTokens,
            UseReferenceAccessTokens = settings.UseReferenceAccessTokens,
            RequireProofKeyForCodeExchange = settings.RequireProofKeyForCodeExchange,
        };

        result.Steps.Add(new JsonObject
        {
            ["name"] = "OpenIdServerSettings",
            ["OpenIdServerSettings"] = JObject.FromObject(settingsModel),
        });
    }
}
