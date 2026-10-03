using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Secrets;
using OrchardCore.Settings;
using OrchardCore.Sms.Azure.Models;

namespace OrchardCore.Sms.Azure.Services;

public sealed class AzureSmsOptionsConfiguration : IConfigureOptions<AzureSmsOptions>
{
    public const string ProtectorName = "AzureSmsProtector";

    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ISecretManager _secretManager;
    private readonly ILogger _logger;

    public AzureSmsOptionsConfiguration(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ISecretManager secretManager,
        ILogger<AzureSmsOptionsConfiguration> logger)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _secretManager = secretManager;
        _logger = logger;
    }

    public void Configure(AzureSmsOptions options)
    {
        var settings = _siteService.GetSettings<AzureSmsSettings>();

        options.IsEnabled = settings.IsEnabled;
        options.PhoneNumber = settings.PhoneNumber;

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
                        _logger.LogDebug("Azure SMS connection string loaded from secret '{SecretName}'.", settings.ConnectionStringSecretName);
                    }

                    return;
                }

                _logger.LogWarning("Azure SMS connection string secret '{SecretName}' was not found or is empty.", settings.ConnectionStringSecretName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Azure SMS connection string from secret '{SecretName}'.", settings.ConnectionStringSecretName);
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
                _logger.LogError(ex, "The Azure SMS connection string could not be decrypted. It may have been encrypted using a different key.");
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
