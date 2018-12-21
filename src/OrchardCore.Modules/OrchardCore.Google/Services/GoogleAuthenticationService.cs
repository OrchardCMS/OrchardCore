using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Google.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Google.Services
{
    public class GoogleAuthenticationService : IGoogleAuthenticationService
    {
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<GoogleAuthenticationService> T;
        private readonly ShellSettings _shellSettings;

        public GoogleAuthenticationService(
            ISiteService siteService,
            ShellSettings shellSettings,
            IStringLocalizer<GoogleAuthenticationService> stringLocalizer)
        {
            _shellSettings = shellSettings;
            _siteService = siteService;
            T = stringLocalizer;
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
            var container = await _siteService.GetSiteSettingsAsync();
            container.Alter<GoogleAuthenticationSettings>(nameof(GoogleAuthenticationSettings), aspect =>
            {
                aspect.ClientID = settings.ClientID;
                aspect.ClientSecret = settings.ClientSecret;
                aspect.CallbackPath = settings.CallbackPath;
            });
            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public IEnumerable<ValidationResult> ValidateSettings(GoogleAuthenticationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (string.IsNullOrWhiteSpace(settings.ClientID))
            {
                yield return new ValidationResult(T["ConsumerKey is required"], new string[] { nameof(settings.ClientID) });
            }

            if (string.IsNullOrWhiteSpace(settings.ClientSecret))
            {
                yield return new ValidationResult(T["ConsumerSecret is required"], new string[] { nameof(settings.ClientSecret) });
            }
        }

    }
}
