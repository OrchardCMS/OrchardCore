using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Secrets;
using OrchardCore.Settings;
using OrchardCore.Twitter.Settings;

namespace OrchardCore.Twitter;

public sealed class Migrations : DataMigration
{
    private readonly ISiteService _siteService;
    private readonly ISecretManager _secretManager;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

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
        await MigrateTwitterSecretsAsync();

        return 1;
    }

    private async Task MigrateTwitterSecretsAsync()
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.As<TwitterSettings>();
        var protector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.Twitter);
        var updated = false;

        // Migrate ConsumerSecret.
#pragma warning disable CS0618 // Type or member is obsolete
        if (!string.IsNullOrWhiteSpace(settings.ConsumerSecret) &&
            string.IsNullOrWhiteSpace(settings.ConsumerSecretSecretName))
        {
            try
            {
                var decryptedSecret = protector.Unprotect(settings.ConsumerSecret);
                var secretName = "Twitter.ConsumerSecret";
                var secret = new TextSecret { Text = decryptedSecret };

                await _secretManager.SaveSecretAsync(secretName, secret);

                settings.ConsumerSecretSecretName = secretName;
                settings.ConsumerSecret = null;
                updated = true;

                _logger.LogInformation("Successfully migrated Twitter consumer secret to Secrets module.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate Twitter consumer secret. The secret may have been encrypted with a different key.");
            }
        }

        // Migrate AccessTokenSecret.
        if (!string.IsNullOrWhiteSpace(settings.AccessTokenSecret) &&
            string.IsNullOrWhiteSpace(settings.AccessTokenSecretSecretName))
        {
            try
            {
                var decryptedSecret = protector.Unprotect(settings.AccessTokenSecret);
                var secretName = "Twitter.AccessTokenSecret";
                var secret = new TextSecret { Text = decryptedSecret };

                await _secretManager.SaveSecretAsync(secretName, secret);

                settings.AccessTokenSecretSecretName = secretName;
                settings.AccessTokenSecret = null;
                updated = true;

                _logger.LogInformation("Successfully migrated Twitter access token secret to Secrets module.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate Twitter access token secret. The secret may have been encrypted with a different key.");
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete

        if (updated)
        {
            site.Put(settings);
            await _siteService.UpdateSiteSettingsAsync(site);
        }
    }
}
