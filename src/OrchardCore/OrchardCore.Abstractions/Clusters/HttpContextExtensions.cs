using Microsoft.AspNetCore.Http;

namespace OrchardCore.Clusters;

/// <summary>
/// Extension methods for checking clusters proxy requests.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Checks if this instance should forward the current request to a tenant cluster,
    /// which is not the case if the request already comes from a clusters proxy.
    /// </summary>
    public static bool AsClustersProxy(this HttpContext context, ClustersOptions options) =>
        options.Enabled && !context.Request.Headers.ContainsKey(RequestHeaderNames.FromClustersProxy);
}
