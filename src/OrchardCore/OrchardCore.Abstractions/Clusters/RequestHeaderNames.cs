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
    /// Request header to prevent proxy requests matching while clustering is disabled.
    /// </summary>
    public static readonly string FakeClustersHeader = "Fake-Clusters-Header";

    /// <summary>
    /// Request header used to retrieve the original host while behind a proxy.
    /// </summary>
    public static readonly string XOriginalHost = "X-Original-Host";
}
