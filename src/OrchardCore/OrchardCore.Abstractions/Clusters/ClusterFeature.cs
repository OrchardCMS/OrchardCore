using OrchardCore.Environment.Shell;

namespace OrchardCore.Clusters;

/// <summary>
/// Used to capture the cluster identifier to target for the current tenant.
/// </summary>
public class ClusterFeature
{
    /// <summary>
    /// The cluster identifier related to the current tenant.
    /// </summary>
    public string ClusterId { get; set; }
}
