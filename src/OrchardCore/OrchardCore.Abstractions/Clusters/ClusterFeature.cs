using OrchardCore.Environment.Shell;

namespace OrchardCore.Clusters;

/// <summary>
/// Used to capture the current <see cref="ShellSettings.TenantId"/>.
/// </summary>
public class ClusterFeature
{
    /// <summary>
    /// The current <see cref="ShellSettings.TenantId"/>.
    /// </summary>
    public string TenantId { get; set; }
}
