using OrchardCore.RemoteManagement;

namespace OrchardCore.Indexing.Endpoints.Management;

internal sealed class IndexRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>([
            new RemoteManagementCapability
            {
                Id = IndexDiscoveryEndpoints.CapabilityName,
                Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                DisplayName = "Indexes",
            },
        ]);
}
