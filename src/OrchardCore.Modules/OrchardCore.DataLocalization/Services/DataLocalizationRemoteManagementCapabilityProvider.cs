using OrchardCore.RemoteManagement;

namespace OrchardCore.DataLocalization.Services;

internal sealed class DataLocalizationRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
        [
            new RemoteManagementCapability
            {
                Id = "localization-translations",
                Version = "1.0",
                DisplayName = "Data Localization",
            },
        ]);
}
