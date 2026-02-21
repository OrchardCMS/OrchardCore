using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Facebook.Settings;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Facebook;

public sealed class Migrations : DataMigration
{
    private const string ProtectorName = FacebookConstants.Features.Core;

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
        await MigrateAppSecretAsync();
        return 1;
    }

    private async Task MigrateAppSecretAsync()
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.As<FacebookSettings>();

#pragma warning disable CS0618 // Type or member is obsolete
        // Skip if no app secret is set or if already migrated
        if (string.IsNullOrWhiteSpace(settings.AppSecret) ||
            !string.IsNullOrWhiteSpace(settings.AppSecretSecretName))
        {
            return;
        }

        try
        {
            var protector = _dataProtectionProvider.CreateProtector(ProtectorName);
            string decryptedAppSecret;

            try
            {
                decryptedAppSecret = protector.Unprotect(settings.AppSecret);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not decrypt Facebook app secret. It may not be encrypted or may be corrupted. Attempting to use as plain text.");
                decryptedAppSecret = settings.AppSecret;
            }

            var secretName = "Facebook.AppSecret";
            var secret = new TextSecret { Text = decryptedAppSecret };

            await _secretManager.SaveSecretAsync(secretName, secret, new SecretSaveOptions
            {
                Description = "Migrated from Facebook settings",
            });

            settings.AppSecretSecretName = secretName;
            settings.AppSecret = null;
#pragma warning restore CS0618 // Type or member is obsolete

            site.Put(settings);
            await _siteService.UpdateSiteSettingsAsync(site);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Facebook app secret migrated to secret '{SecretName}'.", secretName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to migrate Facebook app secret to secrets.");
        }
    }
}
