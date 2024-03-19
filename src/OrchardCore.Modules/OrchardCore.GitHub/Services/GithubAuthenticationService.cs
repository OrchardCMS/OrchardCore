using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.GitHub.Settings;
using OrchardCore.Security.Services;
using OrchardCore.Settings;

namespace OrchardCore.GitHub.Services;

public class GitHubAuthenticationService : OAuthSettingsService<GitHubAuthenticationSettings>, IGitHubAuthenticationService
{
    private readonly ISiteService _siteService;

    public GitHubAuthenticationService(
        ISiteService siteService,
        IStringLocalizer<OAuthSettingsService<GitHubAuthenticationSettings>> stringLocalizer) : base(siteService, stringLocalizer)
    {
        _siteService = siteService;
    }

    public async Task<GitHubAuthenticationSettings> LoadSettingsAsync()
    {
        var container = await _siteService.LoadSiteSettingsAsync();

        return container.As<GitHubAuthenticationSettings>();
    }

    public override IEnumerable<ValidationResult> ValidateSettings(GitHubAuthenticationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.ClientID))
        {
            yield return new ValidationResult(S["ClientID is required"], [nameof(settings.ClientID)]);
        }

        if (string.IsNullOrWhiteSpace(settings.ClientSecret))
        {
            yield return new ValidationResult(S["ClientSecret is required"], [nameof(settings.ClientSecret)]);
        }
    }
}
