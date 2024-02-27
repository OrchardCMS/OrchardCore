using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter.Services;

public class TwitterSettingsConfiguration : IConfigureOptions<TwitterSettings>
{
    private readonly ITwitterSettingsService _twitterSettingsService;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public TwitterSettingsConfiguration(
        ITwitterSettingsService twitterSettingsService,
        ShellSettings shellSettings,
        ILogger<TwitterSettingsConfiguration> logger)
    {
        _twitterSettingsService = twitterSettingsService;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public void Configure(TwitterSettings options)
    {
        var settings = _twitterSettingsService.GetSettingsAsync()
            .GetAwaiter()
            .GetResult();

        if (IsSettingsValid(settings))
        {
            options.ConsumerKey = settings.ConsumerKey;
            options.ConsumerSecret = settings.ConsumerSecret;
            options.AccessToken = settings.AccessToken;
            options.AccessTokenSecret = settings.AccessTokenSecret;
        }
        else
        {
            if (!IsSettingsValid(options))
            {
                _logger.LogWarning("Twitter is not correctly configured.");
            }
        }
    }

    private bool IsSettingsValid(TwitterSettings settings)
    {
        if (_twitterSettingsService.ValidateSettings(settings).Any(result => result != ValidationResult.Success))
        {
            if (_shellSettings.IsRunning())
            {
                return false;
            }
        }

        return true;
    }
}
