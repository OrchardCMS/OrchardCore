using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.OpenId.Services;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using static Orchard.OpenId.Settings.OpenIdSettings;

namespace Orchard.OpenId.Recipes
{
    public class OpenIdSettingsStep : RecipeExecutionStep
    {
        private readonly IOpenIdService _openIdService;

        public OpenIdSettingsStep(
            IOpenIdService openIdService,
            ILogger<OpenIdSettingsStep> logger,
            IStringLocalizer<OpenIdSettingsStep> localizer)
            : base(logger, localizer)
        {
            _openIdService = openIdService;
        }

        public override string Name
        {
            get { return "OpenIdSettings"; }
        }

        public override async Task ExecuteAsync(RecipeExecutionContext context)
        {
            var model = context.RecipeStep.Step.ToObject<OpenIdSettingsStepModel>();

            var settings = await _openIdService.GetOpenIdSettingsAsync();
            settings.TestingModeEnabled = model.TestingModeEnabled;
            settings.AccessTokenFormat = model.AccessTokenFormat;
            settings.Audiences = model.Audiences;
            settings.Authority = model.Authority;
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
    }
}