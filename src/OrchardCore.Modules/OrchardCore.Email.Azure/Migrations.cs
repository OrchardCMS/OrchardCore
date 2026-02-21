using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Email.Services;
using OrchardCore.Entities;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Email.Azure;

public sealed class Migrations : DataMigration
{
    private const string ProtectorName = AzureEmailOptionsConfiguration.ProtectorName;

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
        await MigrateConnectionStringAsync();
        return 1;
    }

    private async Task MigrateConnectionStringAsync()
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.As<AzureEmailSettings>();

#pragma warning disable CS0618 // Type or member is obsolete
        // Skip if no connection string is set or if already migrated
        if (string.IsNullOrWhiteSpace(settings.ConnectionString) ||
            !string.IsNullOrWhiteSpace(settings.ConnectionStringSecretName))
        {
            return;
        }

        try
        {
            var protector = _dataProtectionProvider.CreateProtector(ProtectorName);
            string decryptedConnectionString;

            try
            {
                decryptedConnectionString = protector.Unprotect(settings.ConnectionString);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not decrypt Azure Email connection string. It may not be encrypted or may be corrupted. Attempting to use as plain text.");
                decryptedConnectionString = settings.ConnectionString;
            }

            var secretName = "AzureEmail.ConnectionString";
            var secret = new TextSecret { Text = decryptedConnectionString };

            await _secretManager.SaveSecretAsync(secretName, secret, new SecretSaveOptions
            {
                Description = "Migrated from Azure Email settings",
            });

            settings.ConnectionStringSecretName = secretName;
            settings.ConnectionString = null;
#pragma warning restore CS0618 // Type or member is obsolete

            site.Put(settings);
            await _siteService.UpdateSiteSettingsAsync(site);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Azure Email connection string migrated to secret '{SecretName}'.", secretName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to migrate Azure Email connection string to secrets.");
        }
    }
}
