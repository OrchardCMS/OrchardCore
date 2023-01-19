using Microsoft.Extensions.Options;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Services;

public class TwitterSettingsConfiguration : IConfigureOptions<TwitterSettings>
{
    private readonly ITwitterSettingsService _twitterSettingsService;

    public TwitterSettingsConfiguration(ITwitterSettingsService twitterSettingsService)
    {
        _twitterSettingsService = twitterSettingsService;
    }

    public void Configure(TwitterSettings options)
    {
        var settings = _twitterSettingsService
            .GetSettingsAsync()
            .GetAwaiter()
            .GetResult();

        options.ConsumerKey = settings.ConsumerKey;
        options.ConsumerSecret = settings.ConsumerSecret;
        options.AccessToken = settings.AccessToken;
        options.AccessTokenSecret= settings.AccessTokenSecret;
    }
}
