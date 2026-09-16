namespace OrchardCore.RemoteManagement;

internal sealed class DefaultCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
            [
                new RemoteManagementCapability
                {
                    Id = "remote-management",
                    Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                    DisplayName = "Remote Management",
                },
            ]);
}
