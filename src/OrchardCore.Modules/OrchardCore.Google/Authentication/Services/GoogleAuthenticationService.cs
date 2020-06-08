using System;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Google.Authentication.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Google.Authentication.Services
{
    public class GoogleAuthenticationService
    {
        private readonly ISiteService _siteService;

        public GoogleAuthenticationService(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task<GoogleAuthenticationSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<GoogleAuthenticationSettings>();
        }

        public async Task UpdateSettingsAsync(GoogleAuthenticationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            var container = await _siteService.LoadSiteSettingsAsync();
            container.Alter<GoogleAuthenticationSettings>(nameof(GoogleAuthenticationSettings), aspect =>
            {
                aspect.ClientID = settings.ClientID;
                aspect.ClientSecret = settings.ClientSecret;
                aspect.CallbackPath = settings.CallbackPath;
            });
            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public bool CheckSettings(GoogleAuthenticationSettings settings)
        {
            var obj = new GoogleAuthenticationSettingsViewModel()
            {
                ClientID = settings.ClientID,
                CallbackPath = settings.CallbackPath,
                ClientSecret = settings.ClientSecret
            };
            var vc = new ValidationContext(obj);
            return Validator.TryValidateObject(obj, vc, ImmutableArray.CreateBuilder<ValidationResult>());
        }
    }
}
