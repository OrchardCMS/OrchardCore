using OrchardCore.Layers.Endpoints.Management;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Layers.Services;

internal sealed class LayersRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
        [
            new RemoteManagementCapability
            {
                Id = LayerManagementEndpoints.CapabilityName,
                Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                DisplayName = "Layers",
            },
            new RemoteManagementCapability
            {
                Id = LayerWidgetEndpoints.CapabilityName,
                Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                DisplayName = "Layer Widgets",
            },
        ]);
}
