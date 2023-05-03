namespace OrchardCore.Clusters;

/// <summary>
/// Request header names used by the clusters proxy.
/// </summary>
public class RequestHeaderNames
{
    public static readonly string FromClustersProxy = "From-Clusters-Proxy";
    public static readonly string CheckClustersProxy = "Check-Clusters-Proxy";
    public static readonly string XOriginalHost = "X-Original-Host";
}
