using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes
{
    /// <summary>
    /// This recipe step sets general OpenID Connect settings.
    /// </summary>
    public class OpenIdServerSettingsStep : IRecipeStepHandler
    {
        private readonly IOpenIdServerService _serverService;

        public OpenIdServerSettingsStep(IOpenIdServerService serverService)
            => _serverService = serverService;

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, nameof(OpenIdServerSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<OpenIdServerSettingsStepModel>();
            var settings = await _serverService.LoadSettingsAsync();

            settings.AccessTokenFormat = model.AccessTokenFormat;
            settings.Authority = !String.IsNullOrEmpty(model.Authority) ? new Uri(model.Authority, UriKind.Absolute) : null;

            settings.EncryptionCertificateStoreLocation = model.EncryptionCertificateStoreLocation;
            settings.EncryptionCertificateStoreName = model.EncryptionCertificateStoreName;
            settings.EncryptionCertificateThumbprint = model.EncryptionCertificateThumbprint;

            settings.SigningCertificateStoreLocation = model.SigningCertificateStoreLocation;
            settings.SigningCertificateStoreName = model.SigningCertificateStoreName;
            settings.SigningCertificateThumbprint = model.SigningCertificateThumbprint;

            settings.AuthorizationEndpointPath = model.EnableAuthorizationEndpoint ?
                new PathString("/connect/authorize") : PathString.Empty;
            settings.LogoutEndpointPath = model.EnableLogoutEndpoint ?
                new PathString("/connect/logout") : PathString.Empty;
            settings.TokenEndpointPath = model.EnableTokenEndpoint ?
                new PathString("/connect/token") : PathString.Empty;
            settings.UserinfoEndpointPath = model.EnableUserInfoEndpoint ?
                new PathString("/connect/userinfo") : PathString.Empty;
            settings.IntrospectionEndpointPath = model.EnableIntrospectionEndpoint ?
                new PathString("/connect/introspect") : PathString.Empty;
            settings.RevocationEndpointPath = model.EnableRevocationEndpoint ?
                new PathString("/connect/revoke") : PathString.Empty;

            settings.AllowAuthorizationCodeFlow = model.AllowAuthorizationCodeFlow;
            settings.AllowClientCredentialsFlow = model.AllowClientCredentialsFlow;
            settings.AllowHybridFlow = model.AllowHybridFlow;
            settings.AllowImplicitFlow = model.AllowImplicitFlow;
            settings.AllowPasswordFlow = model.AllowPasswordFlow;
            settings.AllowRefreshTokenFlow = model.AllowRefreshTokenFlow;

            settings.DisableAccessTokenEncryption = model.DisableAccessTokenEncryption;
            settings.DisableRollingRefreshTokens = model.DisableRollingRefreshTokens;
            settings.UseReferenceAccessTokens = model.UseReferenceAccessTokens;
            settings.RequireProofKeyForCodeExchange = model.RequireProofKeyForCodeExchange;

            await _serverService.UpdateSettingsAsync(settings);
        }
    }
}
