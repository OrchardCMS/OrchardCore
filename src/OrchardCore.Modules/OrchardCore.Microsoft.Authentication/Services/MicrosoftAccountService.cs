using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication.Services;

public class MicrosoftAccountService : IMicrosoftAccountService
{
    private readonly ISiteService _siteService;
    protected readonly IStringLocalizer S;

    public MicrosoftAccountService(
        ISiteService siteService,
        IStringLocalizer<MicrosoftAccountService> stringLocalizer)
    {
        _siteService = siteService;
        S = stringLocalizer;
    }

    public Task<MicrosoftAccountSettings> GetSettingsAsync()
        => _siteService.GetSettingsAsync<MicrosoftAccountSettings>();

    public async Task<MicrosoftAccountSettings> LoadSettingsAsync()
    {
        var container = await _siteService.LoadSiteSettingsAsync();
        return container.As<MicrosoftAccountSettings>();
    }

    public async Task UpdateSettingsAsync(MicrosoftAccountSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var container = await _siteService.LoadSiteSettingsAsync();
        container.Alter<MicrosoftAccountSettings>(aspect =>
        {
            aspect.AppId = settings.AppId;
            aspect.AppSecret = settings.AppSecret;
            aspect.CallbackPath = settings.CallbackPath;
        });

        await _siteService.UpdateSiteSettingsAsync(container);
    }

    public IEnumerable<ValidationResult> ValidateSettings(MicrosoftAccountSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

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
