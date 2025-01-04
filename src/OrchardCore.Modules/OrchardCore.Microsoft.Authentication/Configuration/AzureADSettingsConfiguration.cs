using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Configuration;

public sealed class AzureADSettingsConfiguration : IConfigureOptions<AzureADSettings>
{
    private readonly IAzureADService _azureADService;
    private readonly ILogger _logger;

    public AzureADSettingsConfiguration(
        IAzureADService azureADService,
        ILogger<AzureADSettingsConfiguration> logger)
    {
        _azureADService = azureADService;
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
            return null;
        }

        return settings;
    }
}
