using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication.Services
{
    public class MicrosoftAccountService : IMicrosoftAccountService
    {
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer S;

        public MicrosoftAccountService(
            ISiteService siteService,
            IStringLocalizer<MicrosoftAccountService> stringLocalizer)
        {
            _siteService = siteService;
            S = stringLocalizer;
        }

        public async Task<MicrosoftAccountSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<MicrosoftAccountSettings>();
        }

        public async Task UpdateSettingsAsync(MicrosoftAccountSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            var container = await _siteService.LoadSiteSettingsAsync();
            container.Alter<MicrosoftAccountSettings>(nameof(MicrosoftAccountSettings), aspect =>
            {
                aspect.AppId = settings.AppId;
                aspect.AppSecret = settings.AppSecret;
                aspect.CallbackPath = settings.CallbackPath;
            });
            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public IEnumerable<ValidationResult> ValidateSettings(MicrosoftAccountSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (string.IsNullOrWhiteSpace(settings.AppId))
            {
                yield return new ValidationResult(S["AppId is required"], new string[] { nameof(settings.AppId) });
            }

            if (string.IsNullOrWhiteSpace(settings.AppSecret))
            {
                yield return new ValidationResult(S["AppSecret is required"], new string[] { nameof(settings.AppSecret) });
            }
        }
    }
}
