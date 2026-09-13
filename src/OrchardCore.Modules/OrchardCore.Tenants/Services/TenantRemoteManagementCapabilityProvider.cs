using OrchardCore.RemoteManagement;
using OrchardCore.Tenants.Endpoints.Management;

namespace OrchardCore.Tenants.Services;

internal sealed class TenantRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
            [
                new RemoteManagementCapability
                {
                    Id = TenantManagementApiEndpointConventions.CapabilityName,
                    Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                    DisplayName = "Tenants",
                },
            ]);
}
