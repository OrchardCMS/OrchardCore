using System.Net;
using System.Net.Sockets;

namespace OrchardCore.Workflows.Http.Services;

internal static class HttpRequestTaskHttpClient
{
    private static readonly IPNetwork[] s_nonPublicIpv4Networks = ParseNetworks(
        "0.0.0.0/8",
        "10.0.0.0/8",
        "100.64.0.0/10",
        "127.0.0.0/8",
        "169.254.0.0/16",
        "172.16.0.0/12",
        "192.0.0.0/24",
        "192.0.2.0/24",
        "192.88.99.0/24",
        "192.168.0.0/16",
        "198.18.0.0/15",
        "198.51.100.0/24",
        "203.0.113.0/24",
        "224.0.0.0/4",
        "240.0.0.0/4");

    private static readonly IPNetwork[] s_nonPublicIpv6Networks = ParseNetworks(
        "2001::/32",
        "2001:2::/48",
        "2001:10::/28",
        "2001:20::/28",
        "2001:db8::/32",
        "2002::/16",
        "3fff::/20");

    private static readonly IPNetwork s_globalUnicastIpv6Network = IPNetwork.Parse("2000::/3");
    private static readonly IPNetwork s_nat64Network = IPNetwork.Parse("64:ff9b::/96");

    public const string Name = "OrchardCore.Workflows.HttpRequestTask";

    public static SocketsHttpHandler CreateHandler()
    {
        return new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            UseProxy = false,
            ConnectCallback = async (context, cancellationToken) =>
            {
                var endPoint = context.DnsEndPoint;
                var addresses = await Dns.GetHostAddressesAsync(endPoint.Host, endPoint.AddressFamily, cancellationToken);

                if (addresses.Length == 0 || addresses.Any(address => !IsPublicAddress(address)))
                {
                    throw new BlockedDestinationException();
                }

                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = true,
                };

                try
                {
                    await socket.ConnectAsync(addresses, endPoint.Port, cancellationToken);
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            },
        };
    }

    public static bool TryCreateUri(string value, out Uri uri)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || string.IsNullOrEmpty(uri.Host)
            || Uri.CheckHostName(uri.IdnHost) == UriHostNameType.Unknown
            || !string.IsNullOrEmpty(uri.UserInfo)
            || uri.Port is < 1 or > 65535)
        {
            uri = null;
            return false;
        }

        return true;
    }

    public static bool IsBlockedDestination(Exception exception)
    {
        while (exception != null)
        {
            if (exception is BlockedDestinationException)
            {
                return true;
            }

            exception = exception.InnerException;
        }

        return false;
    }

    public static bool IsPublicAddress(IPAddress address)
    {
        address = address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            return !s_nonPublicIpv4Networks.Any(network => network.Contains(address));
        }

        if (address.AddressFamily != AddressFamily.InterNetworkV6)
        {
            return false;
        }

        if (s_nat64Network.Contains(address))
        {
            return IsPublicAddress(new IPAddress(address.GetAddressBytes()[12..]));
        }

        return s_globalUnicastIpv6Network.Contains(address)
            && !s_nonPublicIpv6Networks.Any(network => network.Contains(address));
    }

    private static IPNetwork[] ParseNetworks(params string[] values)
        => values.Select(IPNetwork.Parse).ToArray();

    private sealed class BlockedDestinationException : Exception
    {
    }
}
