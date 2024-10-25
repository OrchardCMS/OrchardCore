using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.GitHub.Settings;
using OrchardCore.Settings;

namespace OrchardCore.GitHub.Services;

public class GitHubAuthenticationService : IGitHubAuthenticationService
{
    private readonly ISiteService _siteService;
    protected readonly IStringLocalizer S;

    public GitHubAuthenticationService(
        ISiteService siteService,
        IStringLocalizer<GitHubAuthenticationService> stringLocalizer)
    {
        _siteService = siteService;
        S = stringLocalizer;
    }

    public Task<GitHubAuthenticationSettings> GetSettingsAsync()
        => _siteService.GetSettingsAsync<GitHubAuthenticationSettings>();

    public async Task<GitHubAuthenticationSettings> LoadSettingsAsync()
    {
        var container = await _siteService.LoadSiteSettingsAsync();
        return container.As<GitHubAuthenticationSettings>();
    }

    public async Task UpdateSettingsAsync(GitHubAuthenticationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var container = await _siteService.LoadSiteSettingsAsync();
        container.Alter<GitHubAuthenticationSettings>(aspect =>
        {
            aspect.ClientID = settings.ClientID;
            aspect.ClientSecret = settings.ClientSecret;
            aspect.CallbackPath = settings.CallbackPath;
        });

        await _siteService.UpdateSiteSettingsAsync(container);
    }

    public IEnumerable<ValidationResult> ValidateSettings(GitHubAuthenticationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.ClientID))
        {
            yield return new ValidationResult(S["ClientID is required"], new string[] { nameof(settings.ClientID) });
        }

        if (string.IsNullOrWhiteSpace(settings.ClientSecret))
        {
            yield return new ValidationResult(S["ClientSecret is required"], new string[] { nameof(settings.ClientSecret) });
        }
    }
}
