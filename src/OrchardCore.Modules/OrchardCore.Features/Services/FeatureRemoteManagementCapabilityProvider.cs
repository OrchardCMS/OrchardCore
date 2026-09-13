using OrchardCore.RemoteManagement;

namespace OrchardCore.Features.Services;

internal sealed class FeatureRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
        [
            new RemoteManagementCapability
            {
                Id = "features",
                Version = "1.0",
                DisplayName = "Features",
            },
        ]);
}
