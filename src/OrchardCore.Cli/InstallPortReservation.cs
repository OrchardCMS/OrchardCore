using System.Net;
using System.Net.Sockets;

namespace OrchardCore.Cli;

/// <summary>Reserves local listen addresses while a site is being installed.</summary>
internal sealed class InstallPortReservation : IDisposable
{
    private readonly List<TcpListener> _listeners;

    private InstallPortReservation(List<TcpListener> listeners, string urls)
    {
        _listeners = listeners;
        Urls = urls;
    }

    public string Urls { get; }

    public static InstallPortReservation Create(string? urls)
    {
        var automatic = urls is null;
        var requested = automatic ? [new Uri("https://localhost:0")] : LocalSiteInstaller.ParseListenUrls(urls!);
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var listeners = new List<TcpListener>();
            var selected = new List<string>();
            var address = requested[0];
            try
            {
                foreach (var uri in requested)
                {
                    address = uri;
                    var port = uri.Port;
                    foreach (var ip in BindAddresses(uri))
                    {
                        var listener = new TcpListener(ip, port);
                        listeners.Add(listener);
                        listener.ExclusiveAddressUse = true;
                        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            listener.Server.DualMode = false;
                        }

                        try
                        {
                            listener.Start();
                        }
                        catch (SocketException exception) when (!IPAddress.TryParse(uri.DnsSafeHost, out _) &&
                            ip.AddressFamily == AddressFamily.InterNetworkV6 &&
                            exception.SocketErrorCode is SocketError.AddressFamilyNotSupported or SocketError.AddressNotAvailable)
                        {
                            // Like Kestrel, allow localhost/wildcard binding on IPv4-only hosts.
                            listener.Stop();
                            continue;
                        }

                        port = ((IPEndPoint)listener.LocalEndpoint).Port;
                    }

                    selected.Add(new UriBuilder(uri) { Port = port }.Uri.GetLeftPart(UriPartial.Authority));
                }

                return new InstallPortReservation(listeners, string.Join(';', selected));
            }
            catch (SocketException exception)
            {
                foreach (var listener in listeners)
                {
                    listener.Stop();
                }

                if (automatic && exception.SocketErrorCode == SocketError.AddressAlreadyInUse && attempt < 19)
                {
                    continue;
                }

                throw new CliException($"Listen address '{address.GetLeftPart(UriPartial.Authority)}' is already in use or unavailable. Choose another port with --urls. {exception.Message}");
            }
            catch
            {
                foreach (var listener in listeners)
                {
                    listener.Stop();
                }

                throw;
            }
        }

        throw new CliException("Could not find an available HTTPS port. Specify a listen address with --urls.");
    }

    private static IEnumerable<IPAddress> BindAddresses(Uri uri)
    {
        if (IPAddress.TryParse(uri.DnsSafeHost, out var ip))
        {
            if (ip.Equals(IPAddress.IPv6Any))
            {
                // Kestrel's IPv6 wildcard uses a dual-mode socket and occupies IPv4 too.
                yield return IPAddress.Any;
            }

            yield return ip;
            yield break;
        }

        var localhost = string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase);
        yield return localhost ? IPAddress.Loopback : IPAddress.Any;
        if (Socket.OSSupportsIPv6)
        {
            // Kestrel binds non-IP host names to all local interfaces, not DNS-resolved addresses.
            yield return localhost ? IPAddress.IPv6Loopback : IPAddress.IPv6Any;
        }
    }

    public void Dispose()
    {
        foreach (var listener in _listeners)
        {
            listener.Stop();
        }
    }
}
