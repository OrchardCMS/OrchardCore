using System;

namespace OrchardCore.Clusters;

/// <summary>
/// Tenant Cluster options.
/// </summary>
public class ClusterOptions
{
    /// <summary>
    /// The cluster identifier.
    /// </summary>
    public string ClusterId { get; set; }

    /// <summary>
    /// The minimum tenant slot number.
    /// </summary>
    public int SlotMin { get; set; }

    /// <summary>
    /// The maximum tenant slot number.
    /// </summary>
    public int SlotMax { get; set; }

    /// <summary>
    /// The maximum idle time before a tenant is released.
    /// </summary>
    public TimeSpan[] MaxIdleTime { get; init; }
}
