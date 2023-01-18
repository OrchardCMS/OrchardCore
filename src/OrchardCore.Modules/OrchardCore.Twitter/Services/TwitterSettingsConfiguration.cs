using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Services;

public class TwitterSettingsConfiguration : IConfigureOptions<TwitterSettings>
{
    private readonly ITwitterSettingsService _twitterSettingsService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public TwitterSettingsConfiguration(
        ITwitterSettingsService twitterSettingsService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<TwitterSettingsConfiguration> logger)
    {
        _twitterSettingsService = twitterSettingsService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public void Configure(TwitterSettings options)
    {
        var settings = _twitterSettingsService
            .GetSettingsAsync()
            .GetAwaiter()
            .GetResult();

        options.ConsumerKey = settings.ConsumerKey;
        options.AccessToken = settings.AccessToken;

        if (!String.IsNullOrWhiteSpace(settings.ConsumerSecret))
        {
            try
            {
                var protector = _dataProtectionProvider.CreateProtector(nameof(TwitterSettingsConfiguration));

                options.ConsumerSecret = protector.Unprotect(settings.ConsumerSecret);
            }
            catch
            {
                _logger.LogError("The Twitter app consumer secret could not be decrypted. It may have been encrypted using a different key.");
            }
        }

        if (!String.IsNullOrWhiteSpace(settings.AccessTokenSecret))
        {
            try
            {
                var protector = _dataProtectionProvider.CreateProtector(nameof(TwitterSettingsConfiguration));

                options.AccessTokenSecret = protector.Unprotect(settings.AccessTokenSecret);
            }
            catch
            {
                _logger.LogError("The Twitter app access token secret could not be decrypted. It may have been encrypted using a different key.");
            }
        }
    }
}
