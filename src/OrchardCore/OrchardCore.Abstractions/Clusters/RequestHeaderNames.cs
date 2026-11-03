namespace OrchardCore.Clusters;

/// <summary>
/// Request headers used by the clusters proxy.
/// </summary>
public static class RequestHeaderNames
{
    /// <summary>
    /// Sent by any clusters proxy request, prevents network loops.
    /// </summary>
    public const string FromClustersProxy = "From-Clusters-Proxy";
}
