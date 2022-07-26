using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Services;

public class TwitterAuthenticationService : ITwitterAuthenticationService
{
    private readonly ISiteService _siteService;
    private readonly IStringLocalizer S;

    public TwitterAuthenticationService(
        ISiteService siteService,
        IStringLocalizer<TwitterAuthenticationService> stringLocalizer)
    {
        _siteService = siteService;
        S = stringLocalizer;
    }

    public async Task<TwitterAuthenticationSettings> GetSettingsAsync()
    {
        var container = await _siteService.GetSiteSettingsAsync();

        return container.As<TwitterAuthenticationSettings>();
    }

    public async Task<TwitterAuthenticationSettings> LoadSettingsAsync()
    {
        var container = await _siteService.LoadSiteSettingsAsync();

        return container.As<TwitterAuthenticationSettings>();
    }

    public async Task UpdateSettingsAsync(TwitterAuthenticationSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        var container = await _siteService.LoadSiteSettingsAsync();

        container.Alter<TwitterAuthenticationSettings>(nameof(TwitterAuthenticationSettings), aspect =>
        {
            aspect.ConsumerKey = settings.ConsumerKey;
            aspect.ConsumerSecret = settings.ConsumerSecret;
            aspect.AccessToken = settings.AccessToken;
            aspect.AccessTokenSecret = settings.AccessTokenSecret;
            aspect.CallbackPath = settings.CallbackPath;
        });

        await _siteService.UpdateSiteSettingsAsync(container);
    }

    public IEnumerable<ValidationResult> ValidateSettings(TwitterAuthenticationSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        if (String.IsNullOrWhiteSpace(settings.ConsumerKey))
        {
            yield return new ValidationResult(S["ConsumerKey is required"], new string[] { nameof(settings.ConsumerKey) });
        }

        if (String.IsNullOrWhiteSpace(settings.ConsumerSecret))
        {
            yield return new ValidationResult(S["ConsumerSecret is required"], new string[] { nameof(settings.ConsumerSecret) });
        }

        if (String.IsNullOrWhiteSpace(settings.AccessToken))
        {
            yield return new ValidationResult(S["Access Token is required"], new string[] { nameof(settings.AccessToken) });
        }

        if (String.IsNullOrWhiteSpace(settings.AccessTokenSecret))
        {
            yield return new ValidationResult(S["Access Token Secret is required"], new string[] { nameof(settings.AccessTokenSecret) });
        }
    }
}
