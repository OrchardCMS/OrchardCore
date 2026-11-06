using System.Security.Cryptography;
using System.Text;
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
        options.Enabled && !IsFromClustersProxy(context, options);

    private static bool IsFromClustersProxy(HttpContext context, ClustersOptions options)
    {
        if (options.ProxySecret is null || !context.Request.Headers.TryGetValue(RequestHeaderNames.FromClustersProxy, out var value))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(value.ToString()), Encoding.UTF8.GetBytes(options.ProxySecret));
    }
}
