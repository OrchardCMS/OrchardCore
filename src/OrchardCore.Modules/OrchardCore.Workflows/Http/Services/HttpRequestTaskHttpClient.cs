using System.Net;
using System.Net.Sockets;

namespace OrchardCore.Workflows.Http.Services;

internal static class HttpRequestTaskHttpClient
{
    public const string Name = "OrchardCore.Workflows.HttpRequestTask";

    public static SocketsHttpHandler CreateHandler(IHttpRequestDestinationValidator destinationValidator)
    {
        return new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false,
            UseProxy = false,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            ConnectCallback = async (context, cancellationToken) =>
            {
                var endPoint = context.DnsEndPoint;
                var addresses = await Dns.GetHostAddressesAsync(endPoint.Host, endPoint.AddressFamily, cancellationToken);

                if (addresses.Length == 0)
                {
                    throw new HttpRequestDestinationNotAllowedException("The destination host did not resolve to an IP address.");
                }

                if (addresses.Any(address => !destinationValidator.IsAllowed(endPoint.Host, address)))
                {
                    throw new HttpRequestDestinationNotAllowedException("The destination host resolved to a prohibited IP address.");
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
}
