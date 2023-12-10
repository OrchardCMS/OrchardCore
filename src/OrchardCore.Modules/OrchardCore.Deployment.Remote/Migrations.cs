using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Models;
using OrchardCore.Secrets.Services;

namespace OrchardCore.Deployment.Remote;

public class Migrations : DataMigration
{
    private readonly ISecretService _secretService;
    private readonly ShellDescriptor _shellDescriptor;
    private readonly RemoteInstanceService _remoteInstanceService;
    private readonly RemoteClientService _remoteClientService;
    private readonly IDataProtector _dataProtector;
    private readonly ILogger _logger;

    public Migrations(
        ISecretService secretService,
        ShellDescriptor shellDescriptor,
        RemoteInstanceService remoteInstanceService,
        RemoteClientService remoteClientService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<Migrations> logger)
    {
        _secretService = secretService;
        _shellDescriptor = shellDescriptor;
        _remoteInstanceService = remoteInstanceService;
        _remoteClientService = remoteClientService;

        _dataProtector = dataProtectionProvider.CreateProtector("OrchardCore.Deployment").ToTimeLimitedDataProtector();

        _logger = logger;
    }

    // New installations don't need to be upgraded, but because there is no initial migration record,
    // 'UpgradeAsync' is called in a new 'CreateAsync' but only if the feature was already installed.
    public async Task<int> CreateAsync()
    {
        if (_shellDescriptor.WasFeatureAlreadyInstalled("OrchardCore.Deployment.Remote"))
        {
            await UpgradeAsync();
        }

        // Shortcut other migration steps on new content definition schemas.
        return 1;
    }

    // Upgrade an existing installation.
#pragma warning disable CS0618 // Type or member is obsolete
    private async Task UpgradeAsync()
    {
        var remoteInstances = (await _remoteInstanceService.LoadRemoteInstanceListAsync()).RemoteInstances;
        foreach (var remoteInstance in remoteInstances)
        {
            var rsaEncryptionSecret = await _secretService.GetOrCreateSecretAsync<RSASecret>(
                $"OrchardCore.Deployment.Remote.RsaEncryptionSecret.{remoteInstance.ClientName}",
                secret =>
                {
                    using var rsa = RSAGenerator.GenerateRSASecurityKey(2048);
                    secret.PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                    secret.KeyType = RSAKeyType.Public;
                });

            var rsaSigningSecret = await _secretService.GetOrCreateSecretAsync<RSASecret>(
                $"OrchardCore.Deployment.Remote.RsaSigningSecret.{remoteInstance.ClientName}",
                secret =>
                {
                    using var rsa = RSAGenerator.GenerateRSASecurityKey(2048);
                    secret.PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                    secret.PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
                    secret.KeyType = RSAKeyType.PublicPrivatePair;
                });

            var apiKeySecret = await _secretService.GetOrCreateSecretAsync<TextSecret>(
                $"OrchardCore.Deployment.Remote.ApiKey.{remoteInstance.ClientName}");

            apiKeySecret.Text = remoteInstance.ApiKey;

            await _secretService.UpdateSecretAsync(rsaEncryptionSecret);
            await _secretService.UpdateSecretAsync(rsaSigningSecret);
            await _secretService.UpdateSecretAsync(apiKeySecret);

            remoteInstance.ApiKey = null;
            await _remoteInstanceService.UpdateRemoteInstanceAsync(remoteInstance);
        }

        var remoteClients = (await _remoteClientService.GetRemoteClientListAsync()).RemoteClients;
        foreach (var remoteClient in remoteClients)
        {
            var rsaEncryptionSecret = await _secretService.GetOrCreateSecretAsync<RSASecret>(
                $"OrchardCore.Deployment.Remote.RsaEncryptionSecret.{remoteClient.ClientName}",
                secret =>
                {
                    using var rsa = RSAGenerator.GenerateRSASecurityKey(2048);
                    secret.PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                    secret.PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
                    secret.KeyType = RSAKeyType.PublicPrivatePair;
                });

            var rsaSigningSecret = await _secretService.GetOrCreateSecretAsync<RSASecret>(
                $"OrchardCore.Deployment.Remote.RsaSigningSecret.{remoteClient.ClientName}",
                secret =>
                {
                    using var rsa = RSAGenerator.GenerateRSASecurityKey(2048);
                    secret.PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                    secret.KeyType = RSAKeyType.Public;
                });

            var apiKeySecret = await _secretService.GetOrCreateSecretAsync<TextSecret>(
                $"OrchardCore.Deployment.Remote.ApiKey.{remoteClient.ClientName}");

            if (remoteClient.ProtectedApiKey?.Length > 0)
            {
                try
                {
                    apiKeySecret.Text = Encoding.UTF8.GetString(_dataProtector.Unprotect(remoteClient.ProtectedApiKey));
                }
                catch
                {
                    _logger.LogError("The Api Key could not be decrypted. It may have been encrypted using a different key.");
                }
            }

            await _secretService.UpdateSecretAsync(rsaEncryptionSecret);
            await _secretService.UpdateSecretAsync(rsaSigningSecret);
            await _secretService.UpdateSecretAsync(apiKeySecret);

            remoteClient.ProtectedApiKey = [];

            await _remoteClientService.TryUpdateRemoteClientAsync(
                remoteClient.Id,
                remoteClient.ClientName,
                apiKeySecret.Text);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
