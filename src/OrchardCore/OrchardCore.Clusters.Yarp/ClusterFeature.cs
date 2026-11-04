namespace OrchardCore.Clusters;

/// <summary>
/// Used to capture the selected tenant cluster identifier, and the tenant it is selected for.
/// </summary>
public sealed class ClusterFeature
{
    /// <summary>
    /// The selected tenant cluster identifier.
    /// </summary>
    public string ClusterId { get; set; }

    /// <summary>
    /// The name of the tenant of the request.
    /// </summary>
    public string TenantName { get; set; }
}
