using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication;

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
        await MigrateMicrosoftAccountSecretAsync();

        return 1;
    }

    private async Task MigrateMicrosoftAccountSecretAsync()
    {
        var site = await _siteService.LoadSiteSettingsAsync();
        var settings = site.As<MicrosoftAccountSettings>();

        // Only migrate if there's a legacy secret and no new secret is configured.
#pragma warning disable CS0618 // Type or member is obsolete
        if (string.IsNullOrWhiteSpace(settings.AppSecret) ||
            !string.IsNullOrWhiteSpace(settings.AppSecretSecretName))
        {
            return;
        }

        try
        {
            var protector = _dataProtectionProvider.CreateProtector(MicrosoftAuthenticationConstants.Features.MicrosoftAccount);
            var decryptedSecret = protector.Unprotect(settings.AppSecret);

            var secretName = "MicrosoftAccount.AppSecret";
            var secret = new TextSecret { Text = decryptedSecret };

            await _secretManager.SaveSecretAsync(secretName, secret);

            settings.AppSecretSecretName = secretName;
            settings.AppSecret = null;
#pragma warning restore CS0618 // Type or member is obsolete

            site.Put(settings);
            await _siteService.UpdateSiteSettingsAsync(site);

            _logger.LogInformation("Successfully migrated Microsoft Account app secret to Secrets module.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to migrate Microsoft Account app secret. The secret may have been encrypted with a different key.");
        }
    }
}
