namespace OrchardCore.RemoteManagement;

/// <summary>
/// Configures a tenant for direct remote management authentication.
/// </summary>
public interface IRemoteManagementTenantConfigurationService
{
    /// <summary>
    /// Configures the current tenant's remote management authentication.
    /// </summary>
    Task ConfigureAsync();
}
