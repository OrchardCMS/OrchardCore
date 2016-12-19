using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.OpenId.Services;
using System.Security.Cryptography.X509Certificates;
using static Orchard.OpenId.Settings.OpenIdSettings;
using System.Collections.Generic;

namespace Orchard.OpenId.Recipes
{
    public class OpenIdSettingsStep : RecipeExecutionStep
    {
        private readonly IOpenIdService _openIdService;
        
        public OpenIdSettingsStep(IOpenIdService openIdService,
            ILogger<OpenIdSettingsStep> logger,
            IStringLocalizer<OpenIdSettingsStep> localizer) : base(logger, localizer)
        {
            _openIdService = openIdService;
        }

        public override string Name
        {
            get { return "OpenIdSettings"; }
        }

        public override async Task ExecuteAsync(RecipeExecutionContext recipeContext)
        {
            var model = recipeContext.RecipeStep.Step.ToObject<OpenIdSettingsStepModel>();

            var openIdSettings = await _openIdService.GetOpenIdSettingsAsync();
            openIdSettings.TestingModeEnabled = model.TestingModeEnabled;
            openIdSettings.DefaultTokenFormat = model.DefaultTokenFormat;
            openIdSettings.EnableTokenEndpoint = model.EnableTokenEndpoint;
            openIdSettings.EnableAuthorizationEndpoint = model.EnableAuthorizationEndpoint;
            openIdSettings.EnableLogoutEndpoint = model.EnableLogoutEndpoint;            
            openIdSettings.EnableUserInfoEndpoint = model.EnableUserInfoEndpoint;
            openIdSettings.AllowPasswordFlow = model.AllowPasswordFlow;
            openIdSettings.AllowClientCredentialsFlow = model.AllowClientCredentialsFlow;
            openIdSettings.AllowAuthorizationCodeFlow = model.AllowAuthorizationCodeFlow;
            openIdSettings.AllowRefreshTokenFlow = model.AllowRefreshTokenFlow;
            openIdSettings.AllowImplicitFlow = model.AllowImplicitFlow;
            openIdSettings.AllowHybridFlow = model.AllowHybridFlow;
            openIdSettings.Audiences = model.Audiences;
            openIdSettings.Authority = model.Authority;
            openIdSettings.CertificateStoreLocation = model.CertificateStoreLocation;
            openIdSettings.CertificateStoreName = model.CertificateStoreName;
            openIdSettings.CertificateThumbPrint = model.CertificateThumbPrint;
            
            await _openIdService.UpdateOpenIdSettingsAsync(openIdSettings);

        }
    }

    public class OpenIdSettingsStepModel
    {
        public bool TestingModeEnabled { get; set; } = false;
        public TokenFormat DefaultTokenFormat { get; set; } = TokenFormat.JWT;
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
        public bool AllowHybridFlow { get; set; }
    }
}