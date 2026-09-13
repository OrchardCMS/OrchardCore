using OrchardCore.RemoteManagement;
using OrchardCore.Templates.Endpoints.Management;

namespace OrchardCore.Templates.Services;

internal sealed class TemplatesRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
        [
            new RemoteManagementCapability
            {
                Id = TemplateManagementEndpointConventions.CapabilityName,
                Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                DisplayName = "Templates",
            },
        ]);
}
