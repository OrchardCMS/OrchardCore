using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Configuration;

public class MicrosoftAccountSettingsConfiguration : IConfigureOptions<MicrosoftAccountSettings>
{
    private readonly IMicrosoftAccountService _microsoftAccountService;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public MicrosoftAccountSettingsConfiguration(
        IMicrosoftAccountService microsoftAccountService,
        ShellSettings shellSettings,
        ILogger<MicrosoftAccountSettingsConfiguration> logger)
    {
        _microsoftAccountService = microsoftAccountService;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public void Configure(MicrosoftAccountSettings options)
    {
        var settings = GetMicrosoftAccountSettingsAsync()
            .GetAwaiter()
            .GetResult();

        if (settings != null)
        {
            options.AppId = settings.AppId;
            options.AppSecret = settings.AppSecret;
            options.CallbackPath = settings.CallbackPath;
            options.SaveTokens = settings.SaveTokens;
        }
    }

    private async Task<MicrosoftAccountSettings> GetMicrosoftAccountSettingsAsync()
    {
        var settings = await _microsoftAccountService.GetSettingsAsync();
        if (_microsoftAccountService.ValidateSettings(settings).Any(result => result != ValidationResult.Success))
        {
            if (_shellSettings.IsRunning())
            {
                _logger.LogWarning("The Microsoft Account Authentication is not correctly configured.");
            }

            return null;
        }

        return settings;
    }
}
