using OrchardCore.RemoteManagement;
using OrchardCore.Shortcodes.Endpoints.Management;

namespace OrchardCore.Shortcodes.Services;

internal sealed class ShortcodeTemplatesRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
        [
            new RemoteManagementCapability
            {
                Id = ShortcodeTemplateManagementEndpoints.CapabilityName,
                Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                DisplayName = "Shortcode Templates",
            },
        ]);
}
