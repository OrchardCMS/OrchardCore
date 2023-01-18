using Microsoft.Extensions.Options;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Configuration;

public class AzureADSettingsConfiguration : IConfigureOptions<AzureADSettings>
{
    private readonly IAzureADService _azureADService;

    public AzureADSettingsConfiguration(IAzureADService azureADService)
    {
        _azureADService = azureADService;
    }

    public void Configure(AzureADSettings options)
    {
        var settings = _azureADService
            .GetSettingsAsync()
            .GetAwaiter()
            .GetResult();

        options.AppId = settings.AppId;
        options.DisplayName = settings.DisplayName;
        options.CallbackPath = settings.CallbackPath;
        options.TenantId = settings.TenantId;
        options.SaveTokens = settings.SaveTokens;
    }
}
