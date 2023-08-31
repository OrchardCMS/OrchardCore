using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.OpenId.Recipes;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Deployment
{
    public class OpenIdServerDeploymentSource : IDeploymentSource
    {
        private readonly IOpenIdServerService _openIdServerService;

        public OpenIdServerDeploymentSource(IOpenIdServerService openIdServerService)
        {
            _openIdServerService = openIdServerService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var openIdServerStep = step as OpenIdServerDeploymentStep;

            if (openIdServerStep == null)
            {
                return;
            }

            var settings = await _openIdServerService
                .GetSettingsAsync();

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
                EnableAuthorizationEndpoint = !String.IsNullOrWhiteSpace(settings.AuthorizationEndpointPath),
                EnableLogoutEndpoint = !String.IsNullOrWhiteSpace(settings.LogoutEndpointPath),
                EnableTokenEndpoint = !String.IsNullOrWhiteSpace(settings.TokenEndpointPath),
                EnableUserInfoEndpoint = !String.IsNullOrWhiteSpace(settings.UserinfoEndpointPath),
                EnableIntrospectionEndpoint = !String.IsNullOrWhiteSpace(settings.IntrospectionEndpointPath),
                EnableRevocationEndpoint = !String.IsNullOrWhiteSpace(settings.RevocationEndpointPath),

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

            // Use nameof(OpenIdServerSettings) as name,
            // to match the recipe step.
            var obj = new JObject(
                new JProperty(
                    "name",
                    nameof(OpenIdServerSettings)));

            obj.Merge(JObject.FromObject(settingsModel));

            result.Steps.Add(obj);
        }
    }
}
