namespace OrchardCore.Clusters;

/// <summary>
/// Request headers used by the clusters proxy.
/// </summary>
public class RequestHeaderNames
{
    /// <summary>
    /// Sent by any clusters proxy request, prevents network loops.
    /// </summary>
    public static readonly string FromClustersProxy = "From-Clusters-Proxy";

    /// <summary>
    /// Prevents proxy requests matching while tenant clusters is disabled.
    /// </summary>
    public static readonly string FakeClustersHeader = "Fake-Clusters-Header";

    /// <summary>
    /// Used to retrieve the original host while behind a clusters proxy.
    /// </summary>
    public static readonly string XOriginalHost = "X-Original-Host";
}
