using OrchardCore.RemoteManagement;

namespace OrchardCore.Users.Services;

internal sealed class UserRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
        [
            new RemoteManagementCapability
            {
                Id = "users",
                Version = "1.0",
                DisplayName = "Users",
            },
        ]);
}
