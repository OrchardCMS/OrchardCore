using OrchardCore.Placements.Endpoints.Management;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Placements.Services;

internal sealed class PlacementsRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
        [
            new RemoteManagementCapability
            {
                Id = PlacementManagementEndpoints.CapabilityName,
                Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                DisplayName = "Placements",
            },
        ]);
}
