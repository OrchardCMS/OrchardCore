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
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (String.IsNullOrWhiteSpace(settings.AppId))
            {
                yield return new ValidationResult(S["AppId is required"], new string[] { nameof(settings.AppId) });
            }

            if (String.IsNullOrWhiteSpace(settings.AppSecret))
            {
                yield return new ValidationResult(S["AppSecret is required"], new string[] { nameof(settings.AppSecret) });
            }
        }
    }
}
