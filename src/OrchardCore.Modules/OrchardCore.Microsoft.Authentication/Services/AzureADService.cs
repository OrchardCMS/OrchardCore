using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Security.Services;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication.Services
{
    public class AzureADService : OAuthSettingsService<AzureADSettings>
    {
        public AzureADService(
            ISiteService siteService,
            IStringLocalizer<OAuthSettingsService<AzureADSettings>> stringLocalizer) : base(siteService, stringLocalizer)
        {
        }

        public override IEnumerable<ValidationResult> ValidateSettings(AzureADSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (String.IsNullOrWhiteSpace(settings.DisplayName))
            {
                yield return new ValidationResult(S["DisplayName is required"], new string[] { nameof(settings.DisplayName) });
            }

            if (String.IsNullOrWhiteSpace(settings.AppId))
            {
                yield return new ValidationResult(S["AppId is required"], new string[] { nameof(settings.AppId) });
            }

            if (String.IsNullOrWhiteSpace(settings.TenantId))
            {
                yield return new ValidationResult(S["TenantId is required"], new string[] { nameof(settings.TenantId) });
            }
        }
    }
}
