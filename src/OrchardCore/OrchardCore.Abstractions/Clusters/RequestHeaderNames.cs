namespace OrchardCore.Clusters;

/// <summary>
/// Request header names used by the clusters proxy.
/// </summary>
public class RequestHeaderNames
{
    /// <summary>
    /// Request header sent by any clusters proxy request.
    /// </summary>
    public static readonly string FromClustersProxy = "From-Clusters-Proxy";

    /// <summary>
    /// Request header used to check clusters proxy requests while they are disabled.
    /// </summary>
    public static readonly string CheckClustersProxy = "Check-Clusters-Proxy";

    /// <summary>
    /// Request header used to retrieve the original host while behind a clusters proxy.
    /// </summary>
    public static readonly string XOriginalHost = "X-Original-Host";
}
