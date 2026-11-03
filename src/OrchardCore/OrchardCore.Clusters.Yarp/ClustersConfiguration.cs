using System;
using System.Collections.Generic;

namespace OrchardCore.Clusters;

/// <summary>
/// Tenant Clusters configuration.
/// </summary>
internal sealed class ClustersConfiguration
{
    /// <summary>
    /// Whether this instance forwards the requests to the tenant clusters.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// The maximum idle time before a tenant is released.
    /// </summary>
    public TimeSpan? MaxIdleTime { get; init; }

    /// <summary>
    /// List of all single <see cref="ClusterConfiguration"/>.
    /// </summary>
    public IReadOnlyDictionary<string, ClusterConfiguration> Clusters { get; init; }
}
