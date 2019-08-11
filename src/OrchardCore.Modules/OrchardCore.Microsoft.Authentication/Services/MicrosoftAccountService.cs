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
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication.Services
{
    public class MicrosoftAccountService : IMicrosoftAccountService
    {
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<MicrosoftAccountService> T;
        private readonly ShellSettings _shellSettings;

        public MicrosoftAccountService(
            ISiteService siteService,
            ShellSettings shellSettings,
            IStringLocalizer<MicrosoftAccountService> stringLocalizer)
        {
            _shellSettings = shellSettings;
            _siteService = siteService;
            T = stringLocalizer;
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
            var container = await _siteService.GetSiteSettingsAsync();
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
                yield return new ValidationResult(T["AppId is required"], new string[] { nameof(settings.AppId) });
            }

            if (string.IsNullOrWhiteSpace(settings.AppSecret))
            {
                yield return new ValidationResult(T["AppSecret is required"], new string[] { nameof(settings.AppSecret) });
            }
        }

    }
}
