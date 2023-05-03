using OrchardCore.Environment.Shell;

namespace OrchardCore.Clusters;

/// <summary>
/// Used to capture the <see cref="ShellSettings.TenantId"/> at the start of the tenant pipeline.
/// </summary>
public class ClusterFeature
{
    /// <summary>
    /// The current <see cref="ShellSettings.TenantId"/>.
    /// </summary>
    public string TenantId { get; set; }
}
