using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Google.Authentication.Services;

public class GoogleAuthenticationService : IGoogleAuthenticationService
{
    private readonly ISiteService _siteService;
    protected readonly IStringLocalizer S;

    public GoogleAuthenticationService(
        ISiteService siteService,
        IStringLocalizer<GoogleAuthenticationService> stringLocalizer)
    {
        _siteService = siteService;
        S = stringLocalizer;
    }

    public async Task<GoogleAuthenticationSettings> GetSettingsAsync()
    {
        var container = await _siteService.GetSiteSettingsAsync();
        return container.As<GoogleAuthenticationSettings>();
    }

    public async Task<GoogleAuthenticationSettings> LoadSettingsAsync()
    {
        var container = await _siteService.LoadSiteSettingsAsync();
        return container.As<GoogleAuthenticationSettings>();
    }

    public async Task UpdateSettingsAsync(GoogleAuthenticationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var container = await _siteService.LoadSiteSettingsAsync();
        container.Alter<GoogleAuthenticationSettings>(aspect =>
        {
            aspect.ClientID = settings.ClientID;
            aspect.ClientSecret = settings.ClientSecret;
            aspect.CallbackPath = settings.CallbackPath;
        });

        await _siteService.UpdateSiteSettingsAsync(container);
    }

    public IEnumerable<ValidationResult> ValidateSettings(GoogleAuthenticationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var results = new List<ValidationResult>();

        if (string.IsNullOrEmpty(settings.ClientID))
        {
            results.Add(new ValidationResult(S["The Client ID is required."], new[]
            {
                nameof(settings.ClientID)
            }));
        }

        if (string.IsNullOrEmpty(settings.ClientSecret))
        {
            results.Add(new ValidationResult(S["The Client Secret is required."], new[]
            {
                nameof(settings.ClientSecret)
            }));
        }

        return results;
    }
}
