using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Configuration;

public class AzureADSettingsConfiguration : IConfigureOptions<AzureADSettings>
{
    private readonly IAzureADService _azureADService;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public AzureADSettingsConfiguration(
        IAzureADService azureADService,
        ShellSettings shellSettings,
        ILogger<AzureADSettingsConfiguration> logger)
    {
        _azureADService = azureADService;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public void Configure(AzureADSettings options)
    {
        var settings = _azureADService.GetSettingsAsync()
            .GetAwaiter()
            .GetResult();

        if (IsSettingsValid(settings))
        {
            options.AppId = settings.AppId;
            options.DisplayName = settings.DisplayName;
            options.CallbackPath = settings.CallbackPath;
            options.TenantId = settings.TenantId;
            options.SaveTokens = settings.SaveTokens;
        }
        else
        {
            if (!IsSettingsValid(options))
            {
                _logger.LogWarning("The AzureAD Authentication is not correctly configured.");
            }
        }
    }

    private bool IsSettingsValid(AzureADSettings settings)
    {
        if (_azureADService.ValidateSettings(settings).Any(result => result != ValidationResult.Success))
        {
            if (_shellSettings.IsRunning())
            {
                return false;
            }
        }

        return true;
    }
}
