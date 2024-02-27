using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Facebook.Settings;

namespace OrchardCore.Facebook.Services;

public class FacebookSettingsConfiguration : IConfigureOptions<FacebookSettings>
{
    private readonly IFacebookService _facebookService;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public FacebookSettingsConfiguration(
        IFacebookService facebookService,
        ShellSettings shellSettings,
        ILogger<FacebookSettingsConfiguration> logger)
    {
        _facebookService = facebookService;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public void Configure(FacebookSettings options)
    {
        var settings = _facebookService.GetSettingsAsync()
            .GetAwaiter()
            .GetResult();

        if (IsSettingsValid(settings))
        {
            options.AppId = settings.AppId;
            options.AppSecret = settings.AppSecret;
            options.Version = settings.Version;
            options.FBInit = settings.FBInit;
            options.FBInitParams = settings.FBInitParams;
            options.SdkJs = settings.SdkJs;
        }
        else
        {
            if (!IsSettingsValid(options))
            {
                _logger.LogWarning("Facebook is not correctly configured.");
            }
        }
    }

    private bool IsSettingsValid(FacebookSettings settings)
    {
        if (_facebookService.ValidateSettings(settings).Any(result => result != ValidationResult.Success))
        {
            if (_shellSettings.IsRunning())
            {
                return false;
            }

        }

        return true;
    }
}
