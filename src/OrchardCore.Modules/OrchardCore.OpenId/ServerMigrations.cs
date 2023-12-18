using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;
using OrchardCore.OpenId.Services;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Models;

namespace OrchardCore.OpenId;

[Feature(OpenIdConstants.Features.Server)]
public class ServerMigrations : DataMigration
{
    private readonly ISecretService _secretService;
    private readonly IOpenIdServerService _openIdServerService;
    private readonly ShellDescriptor _shellDescriptor;

    public ServerMigrations(
        ISecretService secretService,
        IOpenIdServerService openIdServerService,
        ShellDescriptor shellDescriptor,
        ILogger<ServerMigrations> logger)
    {
        _secretService = secretService;
        _openIdServerService = openIdServerService;
        _shellDescriptor = shellDescriptor;
    }

    // New installations don't need to be upgraded, but the feature doesn't have an initial migration record,
    // so 'UpgradeAsync()' is called in a new 'CreateAsync()' but only if this feature was already installed.
    public async Task<int> CreateAsync()
    {
        if (_shellDescriptor.WasFeatureAlreadyInstalled(OpenIdConstants.Features.Server))
        {
            await UpgradeAsync();
        }
        else
        {
            await _secretService.AddSecretAsync<RSASecret>(
                name: ServerSecrets.Encryption,
                configure: (secret, info) =>
                    RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.PublicPrivate));

            await _secretService.AddSecretAsync<RSASecret>(
                name: ServerSecrets.Signing,
                configure: (secret, info) =>
                    RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.PublicPrivate));
        }

        // Shortcut other migration steps on new content definition schemas.
        return 1;
    }

    // Upgrade an existing installation.
    private async Task UpgradeAsync()
    {
        var settings = await _openIdServerService.GetSettingsAsync();

        if (settings.EncryptionCertificateStoreLocation is not null &&
            settings.EncryptionCertificateStoreName is not null &&
            !string.IsNullOrEmpty(settings.EncryptionCertificateThumbprint))
        {
            await _secretService.AddSecretAsync<X509Secret>(
                name: ServerSecrets.Encryption,
                configure: (secret, info) =>
                {
                    secret.StoreLocation = settings.EncryptionCertificateStoreLocation;
                    secret.StoreName = settings.EncryptionCertificateStoreName;
                    secret.Thumbprint = settings.EncryptionCertificateThumbprint;
                });

        }
        else
        {
            await _secretService.AddSecretAsync<RSASecret>(
                name: ServerSecrets.Encryption,
                configure: (secret, info) =>
                    RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.PublicPrivate));
        }

        if (settings.SigningCertificateStoreLocation is not null &&
            settings.SigningCertificateStoreName is not null &&
            !string.IsNullOrEmpty(settings.SigningCertificateThumbprint))
        {
            await _secretService.AddSecretAsync<X509Secret>(
                name: ServerSecrets.Signing,
                configure: (secret, info) =>
                {
                    secret.StoreLocation = settings.SigningCertificateStoreLocation;
                    secret.StoreName = settings.SigningCertificateStoreName;
                    secret.Thumbprint = settings.SigningCertificateThumbprint;
                });

        }
        else
        {
            await _secretService.AddSecretAsync<RSASecret>(
                name: ServerSecrets.Signing,
                configure: (secret, info) =>
                    RSAGenerator.ConfigureRSASecretKeys(secret, RSAKeyType.PublicPrivate));
        }
    }
}
