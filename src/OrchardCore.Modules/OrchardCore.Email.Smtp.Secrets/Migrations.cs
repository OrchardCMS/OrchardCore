using OrchardCore.Data.Migration;
using OrchardCore.Email;
using OrchardCore.Entities;
using OrchardCore.Secrets;
using OrchardCore.Settings;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Email.Smtp.Secrets;

/// <summary>
/// Migrates existing SMTP passwords to the Secrets module.
/// </summary>
public sealed class Migrations : DataMigration
{
    private readonly ISiteService _siteService;
    private readonly ISecretManager _secretManager;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    private const string ProtectorName = "SmtpSettingsConfiguration";

    public Migrations(
        ISiteService siteService,
        ISecretManager secretManager,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<Migrations> logger)
    {
        _siteService = siteService;
        _secretManager = secretManager;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public async Task<int> CreateAsync()
    {
        await MigrateSmtpPasswordAsync();
        return 1;
    }

    private async Task MigrateSmtpPasswordAsync()
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        var smtpSettings = site.GetOrCreate<SmtpSettings>();
        var smtpSecretSettings = site.GetOrCreate<SmtpSecretSettings>();

        // Read from obsolete Password property for migration purposes
#pragma warning disable CS0618 // Type or member is obsolete
        // Skip if no password is set or if already migrated
        if (string.IsNullOrWhiteSpace(smtpSettings.Password) ||
            !string.IsNullOrWhiteSpace(smtpSecretSettings.PasswordSecretName))
        {
            return;
        }

        try
        {
            // Decrypt the password
            var protector = _dataProtectionProvider.CreateProtector(ProtectorName);
            var decryptedPassword = protector.Unprotect(smtpSettings.Password);

            // Create the secret
            var secretName = "Smtp.Password";
            var secret = new TextSecret { Text = decryptedPassword };

            await _secretManager.SaveSecretAsync(secretName, secret, new SecretSaveOptions
            {
                Description = "Migrated from SMTP settings",
            });

            // Update settings to reference the secret and clear the password
            smtpSecretSettings.PasswordSecretName = secretName;
            smtpSettings.Password = null;
#pragma warning restore CS0618 // Type or member is obsolete

            site.Put(smtpSecretSettings);
            site.Put(smtpSettings);
            await _siteService.UpdateSiteSettingsAsync(site);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("SMTP password migrated to secret '{SecretName}'.", secretName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to migrate SMTP password to secrets.");
            throw;
        }
    }
}
