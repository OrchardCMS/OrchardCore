using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
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
        var settings = GetAzureADSettingsAsync()
            .GetAwaiter()
            .GetResult();

        if (settings != null)
        {
            options.AppId = settings.AppId;
            options.DisplayName = settings.DisplayName;
            options.CallbackPath = settings.CallbackPath;
            options.TenantId = settings.TenantId;
            options.SaveTokens = settings.SaveTokens;
        }
    }

    private async Task<AzureADSettings> GetAzureADSettingsAsync()
    {
        var settings = await _azureADService.GetSettingsAsync();

        if (_azureADService.ValidateSettings(settings).Any(result => result != ValidationResult.Success))
        {
            if (_shellSettings.IsRunning())
            {
                _logger.LogWarning("The AzureAD Authentication is not correctly configured.");
            }

            return null;
        }

        return settings;
    }
}
