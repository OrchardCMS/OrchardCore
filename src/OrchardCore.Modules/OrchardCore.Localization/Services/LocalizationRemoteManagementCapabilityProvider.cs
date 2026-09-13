using OrchardCore.RemoteManagement;

namespace OrchardCore.Localization.Services;

internal sealed class LocalizationRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
        [
            new RemoteManagementCapability
            {
                Id = "localization",
                Version = "1.0",
                DisplayName = "Localization",
            },
        ]);
}
