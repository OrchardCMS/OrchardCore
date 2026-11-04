namespace OrchardCore.RemoteManagement;

/// <summary>
/// Configures the current tenant's Pomi application when the CLI feature is enabled.
/// </summary>
public interface IRemoteManagementCliConfigurationService
{
    /// <summary>
    /// Returns whether the CLI application and device flow are configured.
    /// </summary>
    Task<bool> IsConfiguredAsync();

    /// <summary>
    /// Creates or repairs the CLI application and device flow after shared authentication is configured.
    /// </summary>
    Task ConfigureAsync();
}
