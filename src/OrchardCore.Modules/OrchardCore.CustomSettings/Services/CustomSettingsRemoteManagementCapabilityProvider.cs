using OrchardCore.RemoteManagement;

namespace OrchardCore.CustomSettings.Services;

internal sealed class CustomSettingsRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public const string CapabilityName = "custom-settings";

    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
        [
            new RemoteManagementCapability
            {
                Id = CapabilityName,
                Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                DisplayName = "Custom Settings",
            },
        ]);
}
