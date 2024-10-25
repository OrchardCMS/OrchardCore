using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication.Services;

public class AzureADService : IAzureADService
{
    private readonly ISiteService _siteService;
    protected readonly IStringLocalizer S;

    public AzureADService(
        ISiteService siteService,
        IStringLocalizer<AzureADService> stringLocalizer)
    {
        _siteService = siteService;
        S = stringLocalizer;
    }

    public Task<AzureADSettings> GetSettingsAsync()
        => _siteService.GetSettingsAsync<AzureADSettings>();

    public async Task<AzureADSettings> LoadSettingsAsync()
    {
        var container = await _siteService.LoadSiteSettingsAsync();
        return container.As<AzureADSettings>();
    }

    public async Task UpdateSettingsAsync(AzureADSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var container = await _siteService.LoadSiteSettingsAsync();
        container.Alter<AzureADSettings>(aspect =>
        {
            aspect.AppId = settings.AppId;
            aspect.CallbackPath = settings.CallbackPath;
            aspect.DisplayName = settings.DisplayName;
            aspect.TenantId = settings.TenantId;
        });

        await _siteService.UpdateSiteSettingsAsync(container);
    }

    public IEnumerable<ValidationResult> ValidateSettings(AzureADSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.DisplayName))
        {
            yield return new ValidationResult(S["DisplayName is required"], new string[] { nameof(settings.DisplayName) });
        }

        if (string.IsNullOrWhiteSpace(settings.AppId))
        {
            yield return new ValidationResult(S["AppId is required"], new string[] { nameof(settings.AppId) });
        }

        if (string.IsNullOrWhiteSpace(settings.TenantId))
        {
            yield return new ValidationResult(S["TenantId is required"], new string[] { nameof(settings.TenantId) });
        }
    }
}
