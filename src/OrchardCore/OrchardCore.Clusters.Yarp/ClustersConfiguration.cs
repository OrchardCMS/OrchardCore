using System.Collections.Generic;

namespace OrchardCore.Clusters;

/// <summary>
/// Tenant Clusters configuration.
/// </summary>
public class ClustersConfiguration
{
    /// <summary>
    /// Wether tenant clusters is enabled or not.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// The hosts that incoming requests should match.
    /// </summary>
    public string[] Hosts { get; init; }

    /// <summary>
    /// List of all single <see cref="ClusterConfiguration"/>.
    /// </summary>
    public IReadOnlyDictionary<string, ClusterConfiguration> Clusters { get; init; }
}
