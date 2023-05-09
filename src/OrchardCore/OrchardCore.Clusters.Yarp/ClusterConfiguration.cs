using System;

namespace OrchardCore.Clusters;

/// <summary>
/// Tenant Cluster configuration.
/// </summary>
public class ClusterConfiguration
{
    /// <summary>
    /// The tenant slot range that should include 2 limits (min and max).
    /// </summary>
    public int[] SlotRange { get; init; }

    /// <summary>
    /// The maximum idle time before a tenant is released.
    /// </summary>
    public TimeSpan? MaxIdleTime { get; init; }
}
