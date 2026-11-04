namespace OrchardCore.RemoteManagement;

/// <summary>
/// Creates a dedicated application for unattended tenant administration.
/// </summary>
public interface IRemoteManagementClientProvisioningService
{
    /// <summary>
    /// Creates a confidential client with the tenant administrator role. Existing clients are never replaced.
    /// </summary>
    Task CreateAsync(RemoteManagementClientCredentials credentials);
}
