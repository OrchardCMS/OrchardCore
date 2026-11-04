using OrchardCore.RemoteManagement;

namespace OrchardCore.Media.Services;

internal sealed class MediaRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
        [
            new RemoteManagementCapability
            {
                Id = "media",
                Version = "1.0",
                DisplayName = "Media",
            },
        ]);
}
