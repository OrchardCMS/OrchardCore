using OrchardCore.Data.Migration;
using OrchardCore.Entities;
using OrchardCore.OpenId.Settings;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.OpenId.Secrets;

/// <summary>
/// Migrates existing OpenID X509 certificate settings to the Secrets module.
/// </summary>
public sealed class Migrations : DataMigration
{
    private readonly ISiteService _siteService;
    private readonly ISecretManager _secretManager;

    public Migrations(
        ISiteService siteService,
        ISecretManager secretManager)
    {
        _siteService = siteService;
        _secretManager = secretManager;
    }

    public async Task<int> CreateAsync()
    {
        var openIdSettings = _siteService.GetSettings<OpenIdServerSettings>();
        var site = await _siteService.LoadSiteSettingsAsync();
        var openIdSecretSettings = site.As<OpenIdSecretSettings>();
        var hasChanges = false;

        // Migrate signing certificate to X509Secret
        if (!string.IsNullOrEmpty(openIdSettings.SigningCertificateThumbprint) &&
            openIdSettings.SigningCertificateStoreLocation.HasValue &&
            openIdSettings.SigningCertificateStoreName.HasValue &&
            string.IsNullOrEmpty(openIdSecretSettings.SigningKeySecretName))
        {
            var secretName = "OpenId.SigningCertificate";
            var existingSecret = await _secretManager.GetSecretAsync<X509Secret>(secretName);

            if (existingSecret == null)
            {
                var x509Secret = new X509Secret
                {
                    StoreLocation = openIdSettings.SigningCertificateStoreLocation.Value,
                    StoreName = openIdSettings.SigningCertificateStoreName.Value,
                    Thumbprint = openIdSettings.SigningCertificateThumbprint,
                };

                await _secretManager.SaveSecretAsync(secretName, x509Secret, new SecretSaveOptions
                {
                    Description = "Migrated from OpenID Server signing certificate settings",
                });

                openIdSecretSettings.SigningKeySecretName = secretName;
                hasChanges = true;
            }
        }

        // Migrate encryption certificate to X509Secret
        if (!string.IsNullOrEmpty(openIdSettings.EncryptionCertificateThumbprint) &&
            openIdSettings.EncryptionCertificateStoreLocation.HasValue &&
            openIdSettings.EncryptionCertificateStoreName.HasValue &&
            string.IsNullOrEmpty(openIdSecretSettings.EncryptionKeySecretName))
        {
            var secretName = "OpenId.EncryptionCertificate";
            var existingSecret = await _secretManager.GetSecretAsync<X509Secret>(secretName);

            if (existingSecret == null)
            {
                var x509Secret = new X509Secret
                {
                    StoreLocation = openIdSettings.EncryptionCertificateStoreLocation.Value,
                    StoreName = openIdSettings.EncryptionCertificateStoreName.Value,
                    Thumbprint = openIdSettings.EncryptionCertificateThumbprint,
                };

                await _secretManager.SaveSecretAsync(secretName, x509Secret, new SecretSaveOptions
                {
                    Description = "Migrated from OpenID Server encryption certificate settings",
                });

                openIdSecretSettings.EncryptionKeySecretName = secretName;
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            site.Put(openIdSecretSettings);
            await _siteService.UpdateSiteSettingsAsync(site);
        }

        return 1;
    }
}
