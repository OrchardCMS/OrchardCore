using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Services;

public sealed class TwitterSettingsConfiguration : IConfigureOptions<TwitterSettings>
{
    private readonly ITwitterSettingsService _twitterSettingsService;

    public TwitterSettingsConfiguration(ITwitterSettingsService twitterSettingsService)
    {
        _twitterSettingsService = twitterSettingsService;
    }

    public void Configure(TwitterSettings options)
    {
        var settings = GetTwitterSettingsAsync()
            .GetAwaiter()
            .GetResult();

        if (settings != null)
        {
            options.ConsumerKey = settings.ConsumerKey;
            options.ConsumerSecret = settings.ConsumerSecret;
            options.AccessToken = settings.AccessToken;
            options.AccessTokenSecret = settings.AccessTokenSecret;
        }
    }

    private async Task<TwitterSettings> GetTwitterSettingsAsync()
    {
        var settings = await _twitterSettingsService.GetSettingsAsync().ConfigureAwait(false);

        if ((_twitterSettingsService.ValidateSettings(settings)).Any(result => result != ValidationResult.Success))
        {
            return null;
        }

        return settings;
    }
}
