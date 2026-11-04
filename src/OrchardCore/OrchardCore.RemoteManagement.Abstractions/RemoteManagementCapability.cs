namespace OrchardCore.RemoteManagement;

/// <summary>
/// Describes a tenant management capability.
/// </summary>
public sealed class RemoteManagementCapability
{
    /// <summary>
    /// Gets or sets the stable capability identifier.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the capability version.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; }
}
