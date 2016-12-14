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
            ILoggerFactory logger,
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
    }
}