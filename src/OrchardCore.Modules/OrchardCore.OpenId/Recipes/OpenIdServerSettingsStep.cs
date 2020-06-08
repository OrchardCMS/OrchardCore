using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;

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
            if (!string.Equals(context.Name, nameof(OpenIdServerSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<OpenIdServerSettingsStepModel>();

            var settings = await _serverService.GetSettingsAsync();
            settings.AccessTokenFormat = model.AccessTokenFormat;
            settings.Authority = !string.IsNullOrEmpty(model.Authority) ? new Uri(model.Authority, UriKind.Absolute) : null;

            settings.CertificateStoreLocation = model.CertificateStoreLocation;
            settings.CertificateStoreName = model.CertificateStoreName;
            settings.CertificateThumbprint = model.CertificateThumbprint;

            settings.AuthorizationEndpointPath = model.EnableAuthorizationEndpoint ?
                new PathString("/connect/authorize") : PathString.Empty;
            settings.LogoutEndpointPath = model.EnableLogoutEndpoint ?
                new PathString("/connect/logout") : PathString.Empty;
            settings.TokenEndpointPath = model.EnableTokenEndpoint ?
                new PathString("/connect/token") : PathString.Empty;
            settings.UserinfoEndpointPath = model.EnableUserInfoEndpoint ?
                new PathString("/connect/userinfo") : PathString.Empty;

            if (model.AllowImplicitFlow)
            {
                settings.GrantTypes.Add(GrantTypes.Implicit);
            }
            else
            {
                settings.GrantTypes.Remove(GrantTypes.Implicit);
            }

            if (model.AllowClientCredentialsFlow)
            {
                settings.GrantTypes.Add(GrantTypes.ClientCredentials);
            }
            else
            {
                settings.GrantTypes.Remove(GrantTypes.ClientCredentials);
            }

            if (model.AllowPasswordFlow)
            {
                settings.GrantTypes.Add(GrantTypes.Password);
            }
            else
            {
                settings.GrantTypes.Remove(GrantTypes.Password);
            }

            if (model.AllowRefreshTokenFlow)
            {
                settings.GrantTypes.Add(GrantTypes.RefreshToken);
            }
            else
            {
                settings.GrantTypes.Remove(GrantTypes.RefreshToken);
            }

            settings.UseRollingTokens = model.UseRollingTokens;
            settings.UseReferenceTokens = model.UseReferenceTokens;

            await _serverService.UpdateSettingsAsync(settings);
        }
    }
}
