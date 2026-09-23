using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using OrchardCore.Workflows.Http.Models;

namespace OrchardCore.Workflows.Http.Services;

internal sealed class HttpRequestDestinationValidator : IHttpRequestDestinationValidator
{
    private static readonly IPNetwork[] s_nonPublicIpv4Networks =
    [
        IPNetwork.Parse("0.0.0.0/8"),
        IPNetwork.Parse("10.0.0.0/8"),
        IPNetwork.Parse("100.64.0.0/10"),
        IPNetwork.Parse("127.0.0.0/8"),
        IPNetwork.Parse("169.254.0.0/16"),
        IPNetwork.Parse("172.16.0.0/12"),
        IPNetwork.Parse("192.0.0.0/24"),
        IPNetwork.Parse("192.0.2.0/24"),
        IPNetwork.Parse("192.88.99.0/24"),
        IPNetwork.Parse("192.168.0.0/16"),
        IPNetwork.Parse("198.18.0.0/15"),
        IPNetwork.Parse("198.51.100.0/24"),
        IPNetwork.Parse("203.0.113.0/24"),
        IPNetwork.Parse("224.0.0.0/4"),
        IPNetwork.Parse("240.0.0.0/4"),
    ];

    private static readonly IPNetwork s_globalUnicastIpv6Network = IPNetwork.Parse("2000::/3");
    private static readonly IPNetwork s_nat64Network = IPNetwork.Parse("64:ff9b::/96");

    private static readonly IPNetwork[] s_nonPublicIpv6Networks =
    [
        IPNetwork.Parse("2001::/32"),
        IPNetwork.Parse("2001:2::/48"),
        IPNetwork.Parse("2001:10::/28"),
        IPNetwork.Parse("2001:20::/28"),
        IPNetwork.Parse("2001:db8::/32"),
        IPNetwork.Parse("2002::/16"),
        IPNetwork.Parse("3fff::/20"),
    ];

    private readonly bool _allowOnlyConfiguredDestinations;
    private readonly HashSet<string> _allowedHosts;
    private readonly HashSet<IPAddress> _allowedIpAddresses;
    private readonly IPNetwork[] _allowedIpNetworks;

    public HttpRequestDestinationValidator(IOptions<HttpRequestTaskOptions> options)
    {
        var value = options.Value;

        _allowOnlyConfiguredDestinations = value.AllowOnlyConfiguredDestinations;
        _allowedHosts = (value.AllowedHosts ?? [])
            .Where(host => !string.IsNullOrWhiteSpace(host))
            .Select(NormalizeHost)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _allowedIpAddresses = (value.AllowedIpAddresses ?? [])
            .Where(address => !string.IsNullOrWhiteSpace(address))
            .Select(ParseIpAddress)
            .ToHashSet();
        _allowedIpNetworks = (value.AllowedIpNetworks ?? [])
            .Where(network => !string.IsNullOrWhiteSpace(network))
            .Select(ParseIpNetwork)
            .ToArray();
    }

    public bool IsAllowed(string host, IPAddress address)
    {
        address = NormalizeAddress(address);

        var isConfiguredDestination = _allowedHosts.Contains(NormalizeHost(host))
            || _allowedIpAddresses.Contains(address)
            || _allowedIpNetworks.Any(network => network.Contains(address));

        if (isConfiguredDestination)
        {
            return true;
        }

        return !_allowOnlyConfiguredDestinations && IsPublicAddress(address);
    }

    public static bool TryCreateUri(string value, out Uri uri, out string failureReason)
    {
        uri = null;

        if (!Uri.TryCreate(value, UriKind.Absolute, out var parsedUri))
        {
            failureReason = "The URL is not an absolute URI.";
            return false;
        }

        if (!string.Equals(parsedUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(parsedUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            failureReason = "Only HTTP and HTTPS URLs are allowed.";
            return false;
        }

        if (string.IsNullOrEmpty(parsedUri.Host)
            || Uri.CheckHostName(parsedUri.IdnHost) == UriHostNameType.Unknown)
        {
            failureReason = "The URL does not contain a valid host.";
            return false;
        }

        if (!string.IsNullOrEmpty(parsedUri.UserInfo))
        {
            failureReason = "User information is not allowed in the URL.";
            return false;
        }

        if (parsedUri.Port is < 1 or > 65535)
        {
            failureReason = "The URL does not contain a valid port.";
            return false;
        }

        uri = parsedUri;
        failureReason = null;
        return true;
    }

    private static bool IsPublicAddress(IPAddress address)
    {
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

    private static IPAddress NormalizeAddress(IPAddress address)
        => address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;

    private static string NormalizeHost(string host)
    {
        host = host.Trim().TrimEnd('.');

        if (IPAddress.TryParse(host, out var address))
        {
            return NormalizeAddress(address).ToString();
        }

        return new IdnMapping().GetAscii(host);
    }

    private static IPAddress ParseIpAddress(string value)
    {
        if (!IPAddress.TryParse(value.Trim(), out var address))
        {
            throw new InvalidOperationException($"The configured HTTP request task IP address '{value}' is invalid.");
        }

        return NormalizeAddress(address);
    }

    private static IPNetwork ParseIpNetwork(string value)
    {
        if (!IPNetwork.TryParse(value.Trim(), out var network))
        {
            throw new InvalidOperationException($"The configured HTTP request task IP network '{value}' is invalid.");
        }

        return network;
    }
}
