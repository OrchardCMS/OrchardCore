using OrchardCore.RemoteManagement;

namespace OrchardCore.Deployment.Remote.Services;

internal sealed class RemoteDeploymentCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() => ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
    [
        new RemoteManagementCapability
        {
            Id = "deployment-remote", DisplayName = "Remote Deployment",
            Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
        },
    ]);
}
