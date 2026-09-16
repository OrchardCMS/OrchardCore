using OrchardCore.OpenId.Endpoints.Management;
using OrchardCore.RemoteManagement;

namespace OrchardCore.OpenId.Services;

internal sealed class OpenIdManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
        [
            new RemoteManagementCapability
            {
                Id = OpenIdDiscoveryEndpoints.CapabilityName,
                Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                DisplayName = "OpenID Management",
            },
        ]);
}
