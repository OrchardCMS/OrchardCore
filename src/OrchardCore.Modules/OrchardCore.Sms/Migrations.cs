using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Secrets;
using OrchardCore.Settings;
using OrchardCore.Sms.Models;
using OrchardCore.Sms.Services;

namespace OrchardCore.Sms;

public sealed class Migrations : DataMigration
{
    private const string ProtectorName = TwilioSmsProvider.ProtectorName;

    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ISecretManager _secretManager;
    private readonly ILogger _logger;

    public Migrations(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ISecretManager secretManager,
        ILogger<Migrations> logger)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _secretManager = secretManager;
        _logger = logger;
    }

    public async Task<int> CreateAsync()
    {
        await MigrateTwilioAuthTokenAsync();
        return 1;
    }

    private async Task MigrateTwilioAuthTokenAsync()
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.As<TwilioSettings>();

#pragma warning disable CS0618 // Type or member is obsolete
        // Skip if no auth token is set or if already migrated
        if (string.IsNullOrWhiteSpace(settings.AuthToken) ||
            !string.IsNullOrWhiteSpace(settings.AuthTokenSecretName))
        {
            return;
        }

        try
        {
            var protector = _dataProtectionProvider.CreateProtector(ProtectorName);
            string decryptedAuthToken;

            try
            {
                decryptedAuthToken = protector.Unprotect(settings.AuthToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not decrypt Twilio auth token. It may not be encrypted or may be corrupted. Attempting to use as plain text.");
                decryptedAuthToken = settings.AuthToken;
            }

            var secretName = "Twilio.AuthToken";
            var secret = new TextSecret { Text = decryptedAuthToken };

            await _secretManager.SaveSecretAsync(secretName, secret, new SecretSaveOptions
            {
                Description = "Migrated from Twilio settings",
            });

            settings.AuthTokenSecretName = secretName;
            settings.AuthToken = null;
#pragma warning restore CS0618 // Type or member is obsolete

            site.Put(settings);
            await _siteService.UpdateSiteSettingsAsync(site);

            _logger.LogInformation("Twilio auth token migrated to secret '{SecretName}'.", secretName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to migrate Twilio auth token to secrets.");
        }
    }
}
