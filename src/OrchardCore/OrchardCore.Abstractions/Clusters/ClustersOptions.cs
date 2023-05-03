using System;
using System.Collections.Generic;

namespace OrchardCore.Clusters;

/// <summary>
/// Tenant Clusters options.
/// </summary>
public class ClustersOptions
{
    // The identifier of the route template used by the clusters proxy.
    public static readonly string RouteTemplate = nameof(RouteTemplate);

    /// <summary>
    /// Wether tenant clustering is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The uri hosts that incoming requests should match.
    /// </summary>
    public string[] Hosts { get; set; } = Array.Empty<string>();

    /// <summary>
    /// List of <see cref="ClusterOptions"/> for each cluster.
    /// </summary>
    public List<ClusterOptions> Clusters { get; set; } = new();
}
