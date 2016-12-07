using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.Admin;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.OpenId.Services;
using System.Security.Cryptography.X509Certificates;
using static Orchard.OpenId.Settings.OpenIdSettings;

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
            if (model.TestingModeEnabled.HasValue)
            {
                openIdSettings.TestingModeEnabled = model.TestingModeEnabled.Value;
            }
            if (model.DefaultTokenFormat.HasValue)
            {
                openIdSettings.DefaultTokenFormat = model.DefaultTokenFormat.Value;
            }
            if (!string.IsNullOrEmpty(model.Audience))
            {
                openIdSettings.Audience = model.Audience;
            }
            if (!string.IsNullOrEmpty(model.Authority))
            {
                openIdSettings.Authority = model.Authority;
            }
            if (model.CertificateStoreLocation.HasValue)
            {
                openIdSettings.CertificateStoreLocation = model.CertificateStoreLocation.Value;
            }
            if (model.CertificateStoreName.HasValue)
            {
                openIdSettings.CertificateStoreName = model.CertificateStoreName.Value;
            }
            if (!string.IsNullOrEmpty(model.CertificateThumbPrint))
            {
                openIdSettings.CertificateThumbPrint = model.CertificateThumbPrint;
            }
            await _openIdService.UpdateOpenIdSettingsAsync(openIdSettings);

        }
    }

    public class OpenIdSettingsStepModel
    {
        public bool? TestingModeEnabled { get; set; }
        public TokenFormat? DefaultTokenFormat { get; set; }
        public string Authority { get; set; }
        public string Audience { get; set; }
        public StoreLocation? CertificateStoreLocation { get; set; }
        public StoreName? CertificateStoreName { get; set; }
        public string CertificateThumbPrint { get; set; }
    }
}