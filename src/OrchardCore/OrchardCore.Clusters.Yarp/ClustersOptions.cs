
namespace OrchardCore.Clusters;

/// <summary>
/// Tenant Clusters options.
/// </summary>
public sealed class ClustersOptions
{
    /// <summary>
    /// The application configuration section holding the tenant clusters configuration.
    /// </summary>
    public const string ConfigurationSection = "OrchardCore:Clusters";

    /// <summary>
    /// The identifier of the route template used by the clusters proxy.
    /// </summary>
    public const string RouteTemplate = nameof(RouteTemplate);

    /// <summary>
    /// The number of available tenant cluster slots.
    /// </summary>
    public const int SlotsCount = 16384;

    /// <summary>
    /// Whether this instance forwards the requests to the tenant clusters.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The secret sent by this instance with the requests it forwards, and required from another instance
    /// for a request not to be forwarded again. Required when <see cref="Enabled"/> is <c>true</c>.
    /// </summary>
    public string ProxySecret { get; set; }

    /// <summary>
    /// The maximum idle time before a tenant is released on this instance.
    /// </summary>
    public TimeSpan? MaxIdleTime { get; set; }

    /// <summary>
    /// List of all single <see cref="ClusterOptions"/>.
    /// </summary>
    public List<ClusterOptions> Clusters { get; set; } = [];
}
