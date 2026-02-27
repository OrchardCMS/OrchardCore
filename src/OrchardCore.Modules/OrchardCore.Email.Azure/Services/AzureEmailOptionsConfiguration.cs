using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services;

public sealed class AzureEmailOptionsConfiguration : IConfigureOptions<AzureEmailOptions>
{
    public const string ProtectorName = "AzureEmailProtector";

    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ISecretManager _secretManager;
    private readonly ILogger _logger;

    public AzureEmailOptionsConfiguration(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ISecretManager secretManager,
        ILogger<AzureEmailOptionsConfiguration> logger)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _secretManager = secretManager;
        _logger = logger;
    }

    public void Configure(AzureEmailOptions options)
    {
        var settings = _siteService.GetSettings<AzureEmailSettings>();

        options.IsEnabled = settings.IsEnabled;
        options.DefaultSender = settings.DefaultSender;

        // First try to load from secrets
        if (!string.IsNullOrWhiteSpace(settings.ConnectionStringSecretName))
        {
            try
            {
                var secret = _secretManager.GetSecretAsync<TextSecret>(settings.ConnectionStringSecretName)
                    .GetAwaiter()
                    .GetResult();

                if (secret != null && !string.IsNullOrWhiteSpace(secret.Text))
                {
                    options.ConnectionString = secret.Text;

                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Azure Email connection string loaded from secret '{SecretName}'.", settings.ConnectionStringSecretName);
                    }

                    return;
                }

                _logger.LogWarning("Azure Email connection string secret '{SecretName}' was not found or is empty.", settings.ConnectionStringSecretName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Azure Email connection string from secret '{SecretName}'.", settings.ConnectionStringSecretName);
            }
        }

        // Fall back to legacy encrypted connection string
#pragma warning disable CS0618 // Type or member is obsolete
        if (!string.IsNullOrEmpty(settings.ConnectionString))
        {
            try
            {
                var protector = _dataProtectionProvider.CreateProtector(ProtectorName);
                options.ConnectionString = protector.Unprotect(settings.ConnectionString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The Azure Email connection string could not be decrypted. It may have been encrypted using a different key.");
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
