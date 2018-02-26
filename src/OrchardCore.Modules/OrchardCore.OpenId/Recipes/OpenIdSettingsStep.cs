using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using OrchardCore.OpenId.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using static OrchardCore.OpenId.Settings.OpenIdSettings;

namespace OrchardCore.OpenId.Recipes
{
    /// <summary>
    /// This recipe step sets general OpenID Connect settings.
    /// </summary>
    public class OpenIdSettingsStep : IRecipeStepHandler
    {
        private readonly IOpenIdService _openIdService;

        public OpenIdSettingsStep(IOpenIdService openIdService)
        {
            _openIdService = openIdService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "OpenIdSettings", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<OpenIdSettingsStepModel>();

            var settings = await _openIdService.GetOpenIdSettingsAsync();
            settings.TestingModeEnabled = model.TestingModeEnabled;
            settings.AccessTokenFormat = model.AccessTokenFormat;
            settings.Audiences = model.Audiences;
            settings.Authority = model.Authority;
            settings.EnableTokenEndpoint = model.EnableTokenEndpoint;
            settings.EnableAuthorizationEndpoint = model.EnableAuthorizationEndpoint;
            settings.EnableLogoutEndpoint = model.EnableLogoutEndpoint;
            settings.EnableUserInfoEndpoint = model.EnableUserInfoEndpoint;
            settings.AllowPasswordFlow = model.AllowPasswordFlow;
            settings.AllowClientCredentialsFlow = model.AllowClientCredentialsFlow;
            settings.AllowAuthorizationCodeFlow = model.AllowAuthorizationCodeFlow;
            settings.AllowRefreshTokenFlow = model.AllowRefreshTokenFlow;
            settings.AllowImplicitFlow = model.AllowImplicitFlow;
            settings.UseRollingTokens = model.UseRollingTokens;
            settings.CertificateStoreLocation = model.CertificateStoreLocation;
            settings.CertificateStoreName = model.CertificateStoreName;
            settings.CertificateThumbPrint = model.CertificateThumbPrint;

            await _openIdService.UpdateOpenIdSettingsAsync(settings);
        }
    }

    public class OpenIdSettingsStepModel
    {
        public bool TestingModeEnabled { get; set; } = false;
        public TokenFormat AccessTokenFormat { get; set; } = TokenFormat.Encrypted;
        public string Authority { get; set; }
        public IEnumerable<string> Audiences { get; set; }
        public StoreLocation CertificateStoreLocation { get; set; } = StoreLocation.LocalMachine;
        public StoreName CertificateStoreName { get; set; } = StoreName.My;
        public string CertificateThumbPrint { get; set; }
        public bool EnableTokenEndpoint { get; set; }
        public bool EnableAuthorizationEndpoint { get; set; }
        public bool EnableLogoutEndpoint { get; set; }
        public bool EnableUserInfoEndpoint { get; set; }
        public bool AllowPasswordFlow { get; set; }
        public bool AllowClientCredentialsFlow { get; set; }
        public bool AllowAuthorizationCodeFlow { get; set; }
        public bool AllowRefreshTokenFlow { get; set; }
        public bool AllowImplicitFlow { get; set; }
        public bool UseRollingTokens { get; set; }
    }
}