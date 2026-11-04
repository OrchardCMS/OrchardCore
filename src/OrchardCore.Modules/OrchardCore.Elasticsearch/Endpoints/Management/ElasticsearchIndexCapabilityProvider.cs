using OrchardCore.RemoteManagement;

namespace OrchardCore.Elasticsearch.Endpoints.Management;

internal sealed class ElasticsearchIndexCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>([
            new RemoteManagementCapability
            {
                Id = ElasticsearchIndexDefinitionEndpoints.CapabilityName,
                Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                DisplayName = "Elasticsearch content index definitions",
            },
        ]);
}
