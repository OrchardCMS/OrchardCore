using OrchardCore.RemoteManagement;

namespace OrchardCore.Lucene.Endpoints.Management;

internal sealed class LuceneIndexCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>([
            new RemoteManagementCapability
            {
                Id = LuceneIndexDefinitionEndpoints.CapabilityName,
                Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                DisplayName = "Lucene content index definitions",
            },
        ]);
}
