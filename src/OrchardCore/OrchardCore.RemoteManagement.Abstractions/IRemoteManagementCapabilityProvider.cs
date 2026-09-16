namespace OrchardCore.RemoteManagement;

/// <summary>
/// Provides the remote management capabilities enabled for a tenant.
/// </summary>
public interface IRemoteManagementCapabilityProvider
{
    /// <summary>
    /// Returns the capabilities contributed by the current feature.
    /// </summary>
    /// <returns>The contributed capabilities.</returns>
    ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync();
}
