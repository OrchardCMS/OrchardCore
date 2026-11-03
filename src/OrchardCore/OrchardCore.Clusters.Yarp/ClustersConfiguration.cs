
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
    /// The secret sent with the forwarded requests, and required from another instance for a request not to be forwarded again.
    /// </summary>
    public string ProxySecret { get; init; }

    /// <summary>
    /// The maximum idle time before a tenant is released.
    /// </summary>
    public TimeSpan? MaxIdleTime { get; init; }

    /// <summary>
    /// List of all single <see cref="ClusterConfiguration"/>.
    /// </summary>
    public IReadOnlyDictionary<string, ClusterConfiguration> Clusters { get; init; }
}
