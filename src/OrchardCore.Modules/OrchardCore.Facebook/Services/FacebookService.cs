using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Facebook.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Services;

public class FacebookService : IFacebookService
{
    private readonly ISiteService _siteService;

    protected readonly IStringLocalizer S;

    public FacebookService(
        ISiteService siteService,
        IStringLocalizer<FacebookService> stringLocalizer)
    {
        _siteService = siteService;
        S = stringLocalizer;
    }

    public Task<FacebookSettings> GetSettingsAsync()
        => _siteService.GetSettingsAsync<FacebookSettings>();

    public async Task UpdateSettingsAsync(FacebookSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var container = await _siteService.LoadSiteSettingsAsync();
        container.Properties[nameof(FacebookSettings)] = JObject.FromObject(settings);
        await _siteService.UpdateSiteSettingsAsync(container);
    }

    public IEnumerable<ValidationResult> ValidateSettings(FacebookSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var results = new List<ValidationResult>();

        if (string.IsNullOrEmpty(settings.AppId))
        {
            results.Add(new ValidationResult(S["The AppId is required."], new[]
            {
                nameof(settings.AppId),
            }));
        }

        if (string.IsNullOrEmpty(settings.AppSecret))
        {
            results.Add(new ValidationResult(S["The App Secret is required."], new[]
            {
                nameof(settings.AppSecret),
            }));
        }

        return results;
    }
}
