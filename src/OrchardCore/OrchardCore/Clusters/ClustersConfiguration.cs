using System.Collections.Generic;

namespace OrchardCore.Clusters;

/// <summary>
/// Tenant Clusters configuration.
/// </summary>
public class ClustersConfiguration
{
    /// <summary>
    /// Wether tenant clustering is enabled or not.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// The uri hosts that incoming requests should match.
    /// </summary>
    public string[] Hosts { get; init; }

    /// <summary>
    /// List of <see cref="ClusterConfiguration"/> for each cluster.
    /// </summary>
    public IReadOnlyDictionary<string, ClusterConfiguration> Clusters { get; init; }
}
