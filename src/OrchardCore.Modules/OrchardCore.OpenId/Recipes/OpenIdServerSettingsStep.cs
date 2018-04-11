using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using OrchardCore.OpenId.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using static OrchardCore.OpenId.Settings.OpenIdServerSettings;

namespace OrchardCore.OpenId.Recipes
{
    /// <summary>
    /// This recipe step sets general OpenID Connect settings.
    /// </summary>
    public class OpenIdServerSettingsStep : IRecipeStepHandler
    {
        private readonly IOpenIdServerService _serverService;

        public OpenIdServerSettingsStep(IOpenIdServerService serverService)
        {
            _serverService = serverService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "OpenIdServerSettings", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<OpenIdServerSettingsStepModel>();

            var settings = await _serverService.GetSettingsAsync();
            settings.TestingModeEnabled = model.TestingModeEnabled;
            settings.AccessTokenFormat = model.AccessTokenFormat;
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
            settings.CertificateThumbprint = model.CertificateThumbprint;

            await _serverService.UpdateSettingsAsync(settings);
        }
    }

    public class OpenIdServerSettingsStepModel
    {
        public bool TestingModeEnabled { get; set; } = false;
        public TokenFormat AccessTokenFormat { get; set; } = TokenFormat.Encrypted;
        public string Authority { get; set; }
        public StoreLocation CertificateStoreLocation { get; set; } = StoreLocation.LocalMachine;
        public StoreName CertificateStoreName { get; set; } = StoreName.My;
        public string CertificateThumbprint { get; set; }
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