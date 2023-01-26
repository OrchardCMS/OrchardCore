using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell;
using OrchardCore.Twitter.Settings;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.ComponentModel.DataAnnotations;

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
        var settings = await _twitterSettingsService.GetSettingsAsync();

        if ((_twitterSettingsService.ValidateSettings(settings)).Any(result => result != ValidationResult.Success))
        {
            if (_shellSettings.State == TenantState.Running)
            {
                _logger.LogWarning("Twitter is not correctly configured.");
            }

            return null;
        }

        return settings;
    }
}
