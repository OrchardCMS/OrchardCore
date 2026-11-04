using OrchardCore.RemoteManagement;

namespace OrchardCore.AzureAI.Endpoints.Management;

internal sealed class AzureAISearchIndexCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>([
            new RemoteManagementCapability
            {
                Id = AzureAISearchIndexDefinitionEndpoints.CapabilityName,
                Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                DisplayName = "Azure AI Search content index definitions",
            },
        ]);
}
