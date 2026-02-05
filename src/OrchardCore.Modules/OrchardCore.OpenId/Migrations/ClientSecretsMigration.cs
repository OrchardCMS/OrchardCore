using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.OpenId.Configuration;
using OrchardCore.OpenId.Settings;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.OpenId.Migrations;

public sealed class ClientSecretsMigration : DataMigration
{
    private readonly ISiteService _siteService;
    private readonly ISecretManager _secretManager;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public ClientSecretsMigration(
        ISiteService siteService,
        ISecretManager secretManager,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<ClientSecretsMigration> logger)
    {
        _siteService = siteService;
        _secretManager = secretManager;
        _dataProtectionProvider = dataProtectionProvider;
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
        var settings = site.As<OpenIdClientSettings>();

        // Only migrate if there's a legacy secret and no new secret is configured.
#pragma warning disable CS0618 // Type or member is obsolete
        if (string.IsNullOrWhiteSpace(settings.ClientSecret) ||
            !string.IsNullOrWhiteSpace(settings.ClientSecretSecretName))
        {
            return;
        }

        try
        {
            var protector = _dataProtectionProvider.CreateProtector(nameof(OpenIdClientConfiguration));
            var decryptedSecret = protector.Unprotect(settings.ClientSecret);

            var secretName = "OpenIdClient.ClientSecret";
            var secret = new TextSecret { Text = decryptedSecret };

            await _secretManager.SaveSecretAsync(secretName, secret);

            settings.ClientSecretSecretName = secretName;
            settings.ClientSecret = null;
#pragma warning restore CS0618 // Type or member is obsolete

            site.Put(settings);
            await _siteService.UpdateSiteSettingsAsync(site);

            _logger.LogInformation("Successfully migrated OpenID Connect client secret to Secrets module.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to migrate OpenID Connect client secret. The secret may have been encrypted with a different key.");
        }
    }
}
