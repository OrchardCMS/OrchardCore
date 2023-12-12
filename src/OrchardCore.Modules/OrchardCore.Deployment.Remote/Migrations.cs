using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Deployment.Remote;

public class Migrations : DataMigration
{
    private readonly RemoteClientService _remoteClientService;
    private readonly RemoteInstanceService _remoteInstanceService;
    private readonly ShellDescriptor _shellDescriptor;
    private readonly IDataProtector _dataProtector;
    private readonly ILogger _logger;

    public Migrations(
        RemoteClientService remoteClientService,
        RemoteInstanceService remoteInstanceService,
        ShellDescriptor shellDescriptor,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<Migrations> logger)
    {
        _remoteClientService = remoteClientService;
        _remoteInstanceService = remoteInstanceService;
        _shellDescriptor = shellDescriptor;

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
            var apiKey = remoteInstance.ApiKey;
            remoteInstance.ApiKey = null;
            await _remoteInstanceService.UpdateRemoteInstanceAsync(remoteInstance, apiKey);
        }

        var remoteClients = (await _remoteClientService.GetRemoteClientListAsync()).RemoteClients;
        foreach (var remoteClient in remoteClients)
        {
            string apiKey = null;
            try
            {
                apiKey = Encoding.UTF8.GetString(_dataProtector.Unprotect(remoteClient.ProtectedApiKey));
            }
            catch
            {
                _logger.LogError("The Api Key could not be decrypted. It may have been encrypted using a different key.");
            }

            remoteClient.ProtectedApiKey = [];
            await _remoteClientService.UpdateRemoteClientAsync(remoteClient, apiKey);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
