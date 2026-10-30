using Microsoft.AspNetCore.Http;

namespace OrchardCore.Notifications;

/// <summary>
/// Validates and resolves the URLs notifications point to, see <see cref="Notification.Url"/>.
/// </summary>
public static class NotificationUrlHelper
{
    /// <summary>
    /// Gets whether a URL can be stored on a notification: an application-relative URL starting with <c>~/</c>,
    /// a local URL starting with a single <c>/</c>, or an absolute <c>http</c> or <c>https</c> URL.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns><see langword="true"/> when the URL can be stored on a notification.</returns>
    public static bool IsValid(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || url.Any(char.IsControl))
        {
            return false;
        }

        if (url.StartsWith("~/", StringComparison.Ordinal))
        {
            return IsLocalPath(url.AsSpan(1));
        }

        if (url.StartsWith('/'))
        {
            return IsLocalPath(url);
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Resolves the URL of a notification to a URL the browser can follow, by replacing a leading <c>~</c>
    /// with the base path of the tenant.
    /// </summary>
    /// <param name="url">The URL of the notification.</param>
    /// <param name="pathBase">The base path of the tenant.</param>
    /// <returns>The resolved URL, or <see langword="null"/> when the URL is not valid.</returns>
    public static string Resolve(string url, PathString pathBase)
    {
        if (!IsValid(url))
        {
            return null;
        }

        if (url.StartsWith("~/", StringComparison.Ordinal))
        {
            // The base path never ends with a slash, and is empty for a tenant without a URL prefix.
            return pathBase.Value + url[1..];
        }

        return url;
    }

    // A protocol-relative URL ('//host') or a backslash ('/\host') would leave the site.
    private static bool IsLocalPath(ReadOnlySpan<char> path)
        => path.Length == 1 || (path[1] != '/' && path[1] != '\\');
}
