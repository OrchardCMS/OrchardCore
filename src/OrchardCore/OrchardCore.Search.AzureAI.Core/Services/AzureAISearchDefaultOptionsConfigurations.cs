using Azure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI.Services;

public sealed class AzureAISearchDefaultOptionsConfigurations : IConfigureOptions<AzureAISearchDefaultOptions>
{
    public const string ProtectorName = "AzureAISearch";

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

    public void Configure(AzureAISearchDefaultOptions options)
    {
        var fileOptions = _shellConfiguration.GetSection("OrchardCore_AzureAISearch")
            .Get<AzureAISearchDefaultOptions>()
            ?? new AzureAISearchDefaultOptions();

        // This should be called first determine whether the file configs are set or not.
        options.SetFileConfigurationExists(HasConnectionInfo(fileOptions));

        // The 'DisableUIConfiguration' should always be set from the file-options.
        options.DisableUIConfiguration = fileOptions.DisableUIConfiguration;

        options.Analyzers = fileOptions.Analyzers == null || fileOptions.Analyzers.Length == 0
            ? AzureAISearchDefaultOptions.DefaultAnalyzers
            : fileOptions.Analyzers;

        if (fileOptions.DisableUIConfiguration)
        {
            InitializeFromFileSettings(options, fileOptions);
        }
        else
        {
            // At this point, we can allow the user to update the settings from UI.
            var settings = _siteService.GetSettingsAsync<AzureAISearchDefaultSettings>()
                .GetAwaiter()
                .GetResult();

            if (settings.UseCustomConfiguration)
            {
                InitializeFromUISettings(options, settings);
            }
            else
            {
                // At this point, we are allowed to use file configs.
                InitializeFromFileSettings(options, fileOptions);
            }
        }

        options.SetConfigurationExists(HasConnectionInfo(options));
    }

    private static void InitializeFromFileSettings(AzureAISearchDefaultOptions options, AzureAISearchDefaultOptions fileOptions)
    {
        options.IndexesPrefix = fileOptions.IndexesPrefix;
        options.Endpoint = fileOptions.Endpoint;
        options.AuthenticationType = fileOptions.AuthenticationType;
        options.IdentityClientId = fileOptions.IdentityClientId;

        if (!string.IsNullOrWhiteSpace(fileOptions.Credential?.Key))
        {
            options.AuthenticationType = AzureAIAuthenticationType.ApiKey;
            options.Credential = fileOptions.Credential;
        }
    }

    private void InitializeFromUISettings(AzureAISearchDefaultOptions options, AzureAISearchDefaultSettings settings)
    {
        options.IndexesPrefix = null;
        options.Endpoint = settings.Endpoint;
        options.AuthenticationType = settings.AuthenticationType;

        if (settings.AuthenticationType == AzureAIAuthenticationType.ApiKey)
        {
            var protector = _dataProtectionProvider.CreateProtector(ProtectorName);

            options.Credential = new AzureKeyCredential(protector.Unprotect(settings.ApiKey));
        }
        else if (settings.AuthenticationType == AzureAIAuthenticationType.ManagedIdentity)
        {
            options.IdentityClientId = settings.IdentityClientId;
        }
    }

    private static bool HasConnectionInfo(AzureAISearchDefaultOptions options)
    {
        if (string.IsNullOrEmpty(options.Endpoint))
        {
            return false;
        }

        return options.AuthenticationType != AzureAIAuthenticationType.ApiKey ||
            !string.IsNullOrEmpty(options.Credential?.Key);
    }
}
