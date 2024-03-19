using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Security.Services;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication.Services;

public class MicrosoftAccountService : OAuthSettingsService<MicrosoftAccountSettings>, IMicrosoftAccountService
{
    private readonly ISiteService _siteService;

    public MicrosoftAccountService(
        ISiteService siteService,
        IStringLocalizer<OAuthSettingsService<MicrosoftAccountSettings>> stringLocalizer) : base(siteService, stringLocalizer)
    {
        _siteService = siteService;
    }

    public async Task<MicrosoftAccountSettings> LoadSettingsAsync()
    {
        var container = await _siteService.LoadSiteSettingsAsync();

        return container.As<MicrosoftAccountSettings>();
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
