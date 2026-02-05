using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Google;

public sealed class Migrations : DataMigration
{
    private const string ProtectorName = GoogleConstants.Features.GoogleAuthentication;

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
        await MigrateClientSecretAsync();
        return 1;
    }

    private async Task MigrateClientSecretAsync()
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.As<GoogleAuthenticationSettings>();

#pragma warning disable CS0618 // Type or member is obsolete
        // Skip if no client secret is set or if already migrated
        if (string.IsNullOrWhiteSpace(settings.ClientSecret) ||
            !string.IsNullOrWhiteSpace(settings.ClientSecretSecretName))
        {
            return;
        }

        try
        {
            var protector = _dataProtectionProvider.CreateProtector(ProtectorName);
            string decryptedClientSecret;

            try
            {
                decryptedClientSecret = protector.Unprotect(settings.ClientSecret);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not decrypt Google client secret. It may not be encrypted or may be corrupted. Attempting to use as plain text.");
                decryptedClientSecret = settings.ClientSecret;
            }

            var secretName = "Google.ClientSecret";
            var secret = new TextSecret { Text = decryptedClientSecret };

            await _secretManager.SaveSecretAsync(secretName, secret, new SecretSaveOptions
            {
                Description = "Migrated from Google authentication settings",
            });

            settings.ClientSecretSecretName = secretName;
            settings.ClientSecret = null;
#pragma warning restore CS0618 // Type or member is obsolete

            site.Put(settings);
            await _siteService.UpdateSiteSettingsAsync(site);

            _logger.LogInformation("Google client secret migrated to secret '{SecretName}'.", secretName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to migrate Google client secret to secrets.");
        }
    }
}
