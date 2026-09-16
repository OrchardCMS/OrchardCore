using OrchardCore.RemoteManagement;
using OrchardCore.Tenants.Endpoints.Management;

namespace OrchardCore.Tenants.Services;

internal sealed class FeatureProfilesRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>([
            new RemoteManagementCapability
            {
                Id = FeatureProfileManagementEndpoints.CapabilityName,
                Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                DisplayName = "Tenant Feature Profiles",
            },
        ]);
}
