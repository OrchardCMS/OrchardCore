using OrchardCore.ContentLocalization.Endpoints;
using OrchardCore.RemoteManagement;

namespace OrchardCore.ContentLocalization.Services;

internal sealed class ContentLocalizationRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() => ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
    [
        new RemoteManagementCapability
        {
            Id = ContentLocalizationEndpoints.CapabilityName,
            DisplayName = "Content Localizations",
            Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
        },
    ]);
}
