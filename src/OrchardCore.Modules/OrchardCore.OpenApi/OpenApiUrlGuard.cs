using System.Net;

namespace OrchardCore.OpenApi;

/// <summary>
/// Blocks outbound OAuth discovery/token requests from targeting link-local (e.g. cloud
/// metadata endpoints) or private network addresses, since the target URL is supplied by
/// whoever configures the OpenApi settings or calls the test-connection endpoint.
/// Loopback is intentionally allowed: OrchardCore's own multi-tenancy model routinely hosts
/// one tenant's OpenID Connect server as another tenant on the very same host/port, which is
/// a legitimate, first-party configuration rather than an attack target.
/// </summary>
internal static class OpenApiUrlGuard
{
    private static readonly IPNetwork[] _blockedRanges =
    [
        IPNetwork.Parse("10.0.0.0/8"),
        IPNetwork.Parse("172.16.0.0/12"),
        IPNetwork.Parse("192.168.0.0/16"),
        IPNetwork.Parse("169.254.0.0/16"),
        IPNetwork.Parse("fc00::/7"),
        IPNetwork.Parse("fe80::/10"),
    ];

    public static bool IsExternalUrlAllowed(Uri uri, out string reason)
    {
        reason = null;

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            reason = "Only http and https URLs are allowed.";

            return false;
        }

        if (IPAddress.TryParse(uri.Host, out var address) && Array.Exists(_blockedRanges, range => range.Contains(address)))
        {
            reason = "Requests to link-local and private network addresses are not allowed.";

            return false;
        }

        return true;
    }
}
