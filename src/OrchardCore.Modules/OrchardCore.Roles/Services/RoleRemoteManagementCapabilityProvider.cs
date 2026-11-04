using OrchardCore.RemoteManagement;

namespace OrchardCore.Roles.Services;

internal sealed class RoleRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
        [
            new RemoteManagementCapability
            {
                Id = "roles",
                Version = "1.0",
                DisplayName = "Roles",
            },
        ]);
}
