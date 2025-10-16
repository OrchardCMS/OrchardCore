using Azure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Azure.Core;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI.Services;

public sealed class AzureAISearchDefaultOptionsConfigurations : IConfigureOptions<AzureAISearchDefaultOptions>
{
    public const string ProtectorName = "AzureAISearch";

    private readonly IShellConfiguration _shellConfiguration;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IOptionsMonitor<AzureOptions> _optionsMonitor;
    private readonly ISiteService _siteService;

    public AzureAISearchDefaultOptionsConfigurations(
        IShellConfiguration shellConfiguration,
        IDataProtectionProvider dataProtectionProvider,
        IOptionsMonitor<AzureOptions> optionsMonitor,
        ISiteService siteService)
    {
        _shellConfiguration = shellConfiguration;
        _dataProtectionProvider = dataProtectionProvider;
        _optionsMonitor = optionsMonitor;
        _siteService = siteService;
    }

    public void Configure(AzureAISearchDefaultOptions options)
    {
        var fileOptions = _shellConfiguration.GetSection("OrchardCore_AzureAISearch")
            .Get<AzureAISearchDefaultOptions>()
            ?? new AzureAISearchDefaultOptions();

        // first we populate the options from the AzureOptions credentials.
        var azureOptions = _optionsMonitor.Get(fileOptions.CredentialName ?? AzureOptions.DefaultName);

        options.AuthenticationType = azureOptions.AuthenticationType;
        options.ClientId = azureOptions.ClientId;
        options.ApiKey = azureOptions.ApiKey;
        options.Properties = azureOptions.Properties;

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
            var settings = _siteService.GetSettings<AzureAISearchDefaultSettings>();

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
        options.ClientId = fileOptions.ClientId;

        if (!string.IsNullOrWhiteSpace(fileOptions.ApiKey))
        {
            options.AuthenticationType = AzureAuthenticationType.ApiKey;
#pragma warning disable CS0618 // Type or member is obsolete
            options.Credential = fileOptions.Credential;
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }

    private void InitializeFromUISettings(AzureAISearchDefaultOptions options, AzureAISearchDefaultSettings settings)
    {
        options.IndexesPrefix = null;
        options.Endpoint = settings.Endpoint;
        options.AuthenticationType = settings.AuthenticationType;

        if (settings.AuthenticationType == AzureAuthenticationType.ApiKey)
        {
            var protector = _dataProtectionProvider.CreateProtector(ProtectorName);

            var unprotectedApiKey = protector.Unprotect(settings.ApiKey);

            options.ApiKey = unprotectedApiKey;

#pragma warning disable CS0618 // Type or member is obsolete
            options.Credential = new AzureKeyCredential(unprotectedApiKey);
#pragma warning restore CS0618 // Type or member is obsolete
        }
        else if (settings.AuthenticationType == AzureAuthenticationType.ManagedIdentity)
        {
            options.ClientId = settings.IdentityClientId;
        }
    }

    private static bool HasConnectionInfo(AzureAISearchDefaultOptions options)
    {
        if (string.IsNullOrEmpty(options.Endpoint))
        {
            return false;
        }

        return options.ConfigurationExists();
    }
}
