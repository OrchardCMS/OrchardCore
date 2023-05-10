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
    /// Wether tenant clusters is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The uri hosts that incoming requests should match.
    /// </summary>
    public string[] Hosts { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The maximum idle time before a tenant is released.
    /// </summary>
    public TimeSpan? MaxIdleTime { get; set; }

    /// <summary>
    /// List of all single <see cref="ClusterOptions"/>.
    /// </summary>
    public List<ClusterOptions> Clusters { get; set; } = new();
}
