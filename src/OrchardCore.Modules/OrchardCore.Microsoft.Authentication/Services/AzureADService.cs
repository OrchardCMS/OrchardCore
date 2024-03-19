using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Security.Services;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication.Services;

public class AzureADService : OAuthSettingsService<AzureADSettings>, IAzureADService
{
    public AzureADService(
        ISiteService siteService,
        IStringLocalizer<OAuthSettingsService<AzureADSettings>> stringLocalizer) : base(siteService, stringLocalizer)
    {
    }

    public override IEnumerable<ValidationResult> ValidateSettings(AzureADSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.DisplayName))
        {
            yield return new ValidationResult(S["DisplayName is required"], [nameof(settings.DisplayName)]);
        }

        if (string.IsNullOrWhiteSpace(settings.AppId))
        {
            yield return new ValidationResult(S["AppId is required"], [nameof(settings.AppId)]);
        }

        if (string.IsNullOrWhiteSpace(settings.TenantId))
        {
            yield return new ValidationResult(S["TenantId is required"], [nameof(settings.TenantId)]);
        }
    }
}
