using System;
using System.Collections.Generic;

namespace OrchardCore.Clusters;

/// <summary>
/// Tenant Clusters options.
/// </summary>
public class ClustersOptions
{
    /// <summary>
    /// Wether tenant clustering is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Hosts of instances running as clusters proxy.
    /// </summary>
    public string[] Hosts { get; set; } = Array.Empty<string>();

    /// <summary>
    /// List of <see cref="ClusterOptions"/> for each cluster.
    /// </summary>
    public List<ClusterOptions> Clusters { get; set; } = new();
}
