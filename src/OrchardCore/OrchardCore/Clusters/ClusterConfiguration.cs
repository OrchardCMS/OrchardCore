namespace OrchardCore.Clusters;

/// <summary>
/// Tenant Cluster configuration.
/// </summary>
public class ClusterConfiguration
{
    /// <summary>
    /// The tenant slot range that should be configured with 2 integers,
    /// the minimum and the maximum slot numbers that define the range.
    /// </summary>
    public int[] SlotRange { get; init; }
}
