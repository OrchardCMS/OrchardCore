using OrchardCore.RemoteManagement;
using OrchardCore.Themes.Endpoints.Management;

namespace OrchardCore.Themes.Services;

internal sealed class ThemesRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
        [
            new RemoteManagementCapability
            {
                Id = ThemeManagementEndpointConventions.CapabilityName,
                Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                DisplayName = "Themes",
            },
        ]);
}
