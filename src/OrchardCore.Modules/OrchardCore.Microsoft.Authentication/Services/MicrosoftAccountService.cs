using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Security.Services;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication.Services
{
    public class MicrosoftAccountService : OAuthSettingsService<MicrosoftAccountSettings>
    {
        public MicrosoftAccountService(
            ISiteService siteService,
            IStringLocalizer<OAuthSettingsService<MicrosoftAccountSettings>> stringLocalizer) : base(siteService, stringLocalizer)
        {
        }

        public override IEnumerable<ValidationResult> ValidateSettings(MicrosoftAccountSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            if (string.IsNullOrWhiteSpace(settings.AppId))
            {
                yield return new ValidationResult(S["AppId is required"], [nameof(settings.AppId)]);
            }

            if (string.IsNullOrWhiteSpace(settings.AppSecret))
            {
                yield return new ValidationResult(S["AppSecret is required"], [nameof(settings.AppSecret)]);
            }
        }
    }
}
