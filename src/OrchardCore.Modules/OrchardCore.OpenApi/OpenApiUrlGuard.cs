using System.Net;
using System.Net.Sockets;

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
    /// <summary>
    /// Name of the <see cref="IHttpClientFactory"/> client configured with <see cref="ConnectCallbackAsync"/>.
    /// Used for every outbound OAuth discovery/token request the module makes on behalf of a
    /// configured or caller-supplied URL.
    /// </summary>
    public const string HttpClientName = "OrchardCore.OpenApi.OAuthValidation";

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

        if (IPAddress.TryParse(uri.Host, out var address) && IsBlocked(address))
        {
            reason = "Requests to link-local and private network addresses are not allowed.";

            return false;
        }

        return true;
    }

    private static bool IsBlocked(IPAddress address)
        => Array.Exists(_blockedRanges, range => range.Contains(address));

    /// <summary>
    /// A <see cref="SocketsHttpHandler.ConnectCallback"/> that re-validates the actual resolved
    /// IP address at connection time, rather than the literal URL string. <see cref="IsExternalUrlAllowed"/>
    /// only catches literal IP addresses in the configured URL; on its own it can be bypassed by a
    /// hostname that resolves to a blocked address only at request time (DNS rebinding), or by an
    /// initially-allowed host issuing an HTTP redirect to a blocked address (the HttpClient follows
    /// redirects by default, opening a new connection — through this same callback — for each hop).
    /// Resolution and connection use the same address with no second lookup in between, so there is
    /// no time-of-check/time-of-use gap for a rebinding attacker to exploit.
    /// </summary>
    public static async ValueTask<Stream> ConnectCallbackAsync(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, cancellationToken);

        if (addresses.Length == 0)
        {
            throw new InvalidOperationException($"Could not resolve host '{context.DnsEndPoint.Host}'.");
        }

        var address = addresses[0];

        if (IsBlocked(address))
        {
            throw new InvalidOperationException("Requests to link-local and private network addresses are not allowed.");
        }

        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true,
        };

        try
        {
            await socket.ConnectAsync(address, context.DnsEndPoint.Port, cancellationToken);

            return new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();

            throw;
        }
    }
}
