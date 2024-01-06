using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI.Services;

public class AzureAISearchDefaultOptionsConfigurations : IConfigureOptions<AzureAISearchDefaultOptions>
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ISiteService _siteService;

    public AzureAISearchDefaultOptionsConfigurations(
        IShellConfiguration shellConfiguration,
        IDataProtectionProvider dataProtectionProvider,
        ISiteService siteService)
    {
        _shellConfiguration = shellConfiguration;
        _dataProtectionProvider = dataProtectionProvider;
        _siteService = siteService;
    }

    public async void Configure(AzureAISearchDefaultOptions options)
    {
        var ops = _shellConfiguration.GetSection("OrchardCore_AzureAISearch").Get<AzureAISearchDefaultOptions>();

        if (ops is not null)
        {
            // These settings should always be consumed from the app settings file never the UI.
            options.ConfigurationType = ops.ConfigurationType;
        }

        options.Analyzers = ops?.Analyzers == null || ops.Analyzers.Length == 0
            ? AzureAISearchDefaultOptions.DefaultAnalyzers
            : ops.Analyzers;

        if (options.ConfigurationType == AzureAIConfigurationType.File)
        {
            LoadFileSettings(options, ops);
        }
        else
        {
            // At this point, we can allow the user to update the settings from UI.

            var site = await _siteService.GetSiteSettingsAsync();
            var settings = site.As<AzureAISearchDefaultSettings>();

            if (options.ConfigurationType == AzureAIConfigurationType.UI || settings.UseCustomConfiguration)
            {
                LoadUISettings(options, settings);
            }
            else
            {
                // At this point, we are allowed to use file configs.
                LoadFileSettings(options, ops);
            }
        }

        options.SetConfigurationExists(!string.IsNullOrEmpty(options.Endpoint));
    }

    private static void LoadFileSettings(AzureAISearchDefaultOptions options, AzureAISearchDefaultOptions ops)
    {
        options.IndexesPrefix = ops.IndexesPrefix;
        options.Endpoint = ops.Endpoint;
        options.AuthenticationType = ops.AuthenticationType;

        if (ops.AuthenticationType == AzureAIAuthenticationType.ApiKey || !string.IsNullOrWhiteSpace(ops.Credential?.Key))
        {
            options.AuthenticationType = AzureAIAuthenticationType.ApiKey;
            options.Credential = ops.Credential;
        }
    }

    private void LoadUISettings(AzureAISearchDefaultOptions options, AzureAISearchDefaultSettings settings)
    {
        options.IndexesPrefix = null;
        options.Endpoint = settings.Endpoint;
        options.AuthenticationType = settings.AuthenticationType;

        if (settings.AuthenticationType == AzureAIAuthenticationType.ApiKey)
        {
            var protector = _dataProtectionProvider.CreateProtector("AzureAISearch");

            options.Credential = new Azure.AzureKeyCredential(protector.Unprotect(settings.ApiKey));
        }
    }
}
