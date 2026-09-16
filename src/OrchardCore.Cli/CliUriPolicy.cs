namespace OrchardCore.Cli;

internal static class CliUriPolicy
{
    public static Uri RequireSecureEndpoint(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Fragment) ||
            (uri.Scheme != Uri.UriSchemeHttps && !(uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback)))
        {
            throw new CliException("Endpoints must use HTTPS without embedded credentials or fragments. HTTP is allowed only for loopback development tenants.");
        }

        return uri;
    }

    public static Uri RequireSameOrigin(Uri trusted, string value)
    {
        var uri = RequireSecureEndpoint(value);
        if (!string.Equals(trusted.Scheme, uri.Scheme, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(trusted.IdnHost, uri.IdnHost, StringComparison.OrdinalIgnoreCase) || trusted.Port != uri.Port)
        {
            throw new CliException("The discovered endpoint is outside the trusted origin. Check the tenant's public URL and reverse-proxy configuration.");
        }

        return uri;
    }

    public static Uri RequireTenantEndpoint(Uri tenant, string value)
    {
        var uri = RequireSameOrigin(tenant, value);
        var prefix = tenant.AbsolutePath.TrimEnd('/') + "/";
        if (!uri.AbsolutePath.StartsWith(prefix, StringComparison.Ordinal))
        {
            throw new CliException("The request endpoint is outside the selected tenant's URL path.");
        }

        // Reject encoded separators/dot segments that proxies and servers can normalize differently.
        var decoded = Uri.UnescapeDataString(uri.AbsolutePath);
        if (decoded.Contains('\\') || decoded.Contains('%') ||
            decoded.Split('/').Any(segment => segment is "." or "..") ||
            uri.AbsolutePath.Contains("%2f", StringComparison.OrdinalIgnoreCase))
        {
            throw new CliException("The request path contains ambiguous encoded path segments.");
        }

        return uri;
    }
}
