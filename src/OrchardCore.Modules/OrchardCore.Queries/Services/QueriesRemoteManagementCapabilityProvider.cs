using OrchardCore.RemoteManagement;

namespace OrchardCore.Queries.Services;

internal sealed class QueriesRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
        [
            new RemoteManagementCapability
            {
                Id = "queries",
                Version = "1.0",
                DisplayName = "Queries",
            },
        ]);
}
